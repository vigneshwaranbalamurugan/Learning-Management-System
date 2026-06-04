using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseBatchRepository _batchRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EnrollmentService> _logger;

        public EnrollmentService(
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            ICourseBatchRepository batchRepository,
            IMapper mapper,
            ILogger<EnrollmentService> logger)
        {
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _batchRepository = batchRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<EnrollmentResponse> EnrollAsync(int userId, int courseId, int? batchId)
        {
            // 1. Verify course exists and is published
            var course = await _courseRepository.GetByIdAsync(courseId);

            if (course.Status != CourseStatus.Published)
                throw new InvalidOperationException($"Course '{courseId}' is not published and cannot be enrolled in.");

            // 2. Prevent duplicate enrollment
            var existing = await _enrollmentRepository.GetByUserAndCourseAsync(userId, courseId);
            if (existing != null)
                throw new InvalidOperationException($"User '{userId}' is already enrolled in course '{courseId}'.");

            var enrollment = new Enrollments
            {
                UserId = userId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow,
                EnrollmentStatus = EnrollmentStatus.Active,
                ProgressPercentage = 0,
                IsCompleted = false
            };

            // 3. Branch by access type
            if (course.CourseAccessType == CourseAccessType.SelfPaced)
            {
                await EnrollSelfPacedAsync(enrollment, course, batchId);
            }
            else
            {
                await EnrollCohortBasedAsync(enrollment, course, batchId);
            }

            await _enrollmentRepository.AddAsync(enrollment);

            // 4. Reload with navigations for mapping
            var saved = await _enrollmentRepository.GetByUserAndCourseAsync(userId, courseId)
                        ?? throw new InvalidOperationException("Enrollment could not be retrieved after creation.");

            _logger.LogInformation("Student Enrolled: UserId={UserId}, CourseId={CourseId}, BatchId={BatchId}",
                userId, courseId, batchId?.ToString() ?? "None");

            return _mapper.Map<EnrollmentResponse>(saved);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EnrollmentResponse>> GetMyEnrollmentsAsync(int userId)
        {
            var enrollments = await _enrollmentRepository.GetEnrollmentsByUserAsync(userId);
            return _mapper.Map<IEnumerable<EnrollmentResponse>>(enrollments);
        }

        // ─── Private helpers ──────────────────────────────────────────────────

        private static Task EnrollSelfPacedAsync(
            Enrollments enrollment,
            Courses course,
            int? batchId)
        {
            // SelfPaced must not have a batch
            if (batchId.HasValue)
                throw new InvalidOperationException(
                    "SelfPaced courses do not use batches. Remove the BatchId from the request.");

            enrollment.BatchId = null;

            // Auto-calculate AccessExpiresAt if the course has a deadline configured
            if (course.DefaultAssignmentDeadlineDays.HasValue)
            {
                enrollment.AccessExpiresAt = enrollment.EnrolledAt
                    .AddDays(course.DefaultAssignmentDeadlineDays.Value);
            }
            else
            {
                enrollment.AccessExpiresAt = null; // never expires
            }

            return Task.CompletedTask;
        }

        private async Task EnrollCohortBasedAsync(
            Enrollments enrollment,
            Courses course,
            int? batchId)
        {
            // CohortBased requires a batch
            if (!batchId.HasValue)
                throw new InvalidOperationException(
                    "CohortBased courses require a BatchId to enroll.");

            var batch = await _batchRepository.GetByIdAsync(batchId.Value);

            // Batch must belong to the requested course
            if (batch.CourseId != course.Id)
                throw new InvalidOperationException(
                    $"Batch '{batchId}' does not belong to course '{course.Id}'.");

            // Batch must be in the enrollment window
            var now = DateTime.UtcNow;
            if (now < batch.EnrollmentStartDate || now > batch.EnrollmentEndDate)
                throw new InvalidOperationException(
                    $"Enrollment window for batch '{batchId}' is not currently open.");

            // Seat availability check via PostgreSQL function
            var availableSeats = await _batchRepository.GetAvailableSeatsAsync(batchId.Value);
            if (availableSeats <= 0)
                throw new InvalidOperationException(
                    $"Batch '{batchId}' is full. No seats available.");

            enrollment.BatchId = batchId.Value;
            // Access expires when the batch ends
            enrollment.AccessExpiresAt = batch.EndDate;

            _logger.LogInformation("Student Enrolled In Batch: UserId={UserId}, BatchId={BatchId}, CourseId={CourseId}",
                enrollment.UserId, batchId, course.Id);
        }
    }
}
