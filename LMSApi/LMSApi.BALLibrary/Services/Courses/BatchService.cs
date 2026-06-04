using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services
{
    public class BatchService : IBatchService
    {
        private readonly ICourseBatchRepository _batchRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BatchService> _logger;

        public BatchService(
            ICourseBatchRepository batchRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<BatchService> logger)
        {
            _batchRepository = batchRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<BatchResponse> CreateBatchAsync(int courseId, CreateBatchRequest request)
        {
            // Validate course exists
            var course = await _courseRepository.GetByIdAsync(courseId);

            // Enforce: only CohortBased courses can have batches
            if (course.CourseAccessType != CourseAccessType.CohortBased)
                throw new InvalidOperationException(
                    $"Course '{courseId}' is not CohortBased. Only CohortBased courses support batches.");

            ValidateBatchDates(request.StartDate, request.EndDate,
                               request.EnrollmentStartDate, request.EnrollmentEndDate);

            var batch = _mapper.Map<CourseBatch>(request);
            batch.CourseId = courseId;
            batch.Status = BatchStatus.Upcoming;

            await _batchRepository.AddAsync(batch);

            _logger.LogInformation("Batch Created: '{Name}' for CourseId={CourseId}, BatchId={BatchId}",
                batch.Name, courseId, batch.Id);

            // Fetch again to populate AvailableSeats via PG function
            batch.AvailableSeats = await _batchRepository.GetAvailableSeatsAsync(batch.Id);

            return _mapper.Map<BatchResponse>(batch);
        }

        /// <inheritdoc/>
        public async Task<BatchResponse> UpdateBatchAsync(int batchId, UpdateBatchRequest request)
        {
            var batch = await _batchRepository.GetByIdAsync(batchId);

            if (request.Name != null) batch.Name = request.Name;
            if (request.StartDate.HasValue) batch.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue) batch.EndDate = request.EndDate.Value;
            if (request.EnrollmentStartDate.HasValue) batch.EnrollmentStartDate = request.EnrollmentStartDate.Value;
            if (request.EnrollmentEndDate.HasValue) batch.EnrollmentEndDate = request.EnrollmentEndDate.Value;
            if (request.MaxStudents.HasValue)
            {
                var availableSeats = await _batchRepository.GetAvailableSeatsAsync(batch.Id);
                var currentlyEnrolled = batch.MaxStudents - availableSeats;

                if (request.MaxStudents.Value < currentlyEnrolled)
                {
                    throw new InvalidOperationException($"Cannot reduce MaxStudents to {request.MaxStudents.Value} because {currentlyEnrolled} students are already enrolled.");
                }

                batch.MaxStudents = request.MaxStudents.Value;
            }

            if (request.Status.HasValue)
            {
                var oldStatus = batch.Status;
                batch.Status = request.Status.Value;

                if (request.Status.Value == BatchStatus.Completed && oldStatus != BatchStatus.Completed)
                    _logger.LogInformation("Batch Completed: BatchId={BatchId}, CourseId={CourseId}",
                        batchId, batch.CourseId);
            }

            await _batchRepository.UpdateAsync(batch);

            _logger.LogInformation("Batch Updated: BatchId={BatchId}", batchId);

            batch.AvailableSeats = await _batchRepository.GetAvailableSeatsAsync(batch.Id);

            return _mapper.Map<BatchResponse>(batch);
        }

        /// <inheritdoc/>
        public async Task DeleteBatchAsync(int batchId)
        {
            await _batchRepository.GetByIdAsync(batchId); // throws KeyNotFoundException if missing
            await _batchRepository.DeleteAsync(batchId);
            _logger.LogInformation("Batch Deleted: BatchId={BatchId}", batchId);
        }

        /// <inheritdoc/>
        public async Task<BatchResponse> GetBatchByIdAsync(int batchId)
        {
            var batch = await _batchRepository.GetByIdAsync(batchId);
            batch.AvailableSeats = await _batchRepository.GetAvailableSeatsAsync(batch.Id);
            return _mapper.Map<BatchResponse>(batch);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BatchResponse>> GetBatchesByCourseAsync(int courseId)
        {
            var batches = await _batchRepository.GetBatchesByCourseAsync(courseId);
            return _mapper.Map<IEnumerable<BatchResponse>>(batches);
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static void ValidateBatchDates(
            DateTime start, DateTime end,
            DateTime enrollStart, DateTime enrollEnd)
        {
            if (end <= start)
                throw new ArgumentException("Batch EndDate must be after StartDate.");

            if (enrollEnd <= enrollStart)
                throw new ArgumentException("Enrollment EndDate must be after EnrollmentStartDate.");

            if (enrollEnd > start)
                throw new ArgumentException("Enrollment window must close before the batch starts.");
        }
    }
}
