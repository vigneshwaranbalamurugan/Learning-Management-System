using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IAssignmentSubmissionRepository _submissionRepository;
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IStudentProgressService _progressService;
        private readonly IMapper _mapper;
        private readonly ILogger<AssignmentService> _logger;

        public AssignmentService(
            IAssignmentRepository assignmentRepository,
            IAssignmentSubmissionRepository submissionRepository,
            ICourseSectionRepository sectionRepository,
            IEnrollmentRepository enrollmentRepository,
            IStudentProgressService progressService,
            IMapper mapper,
            ILogger<AssignmentService> logger)
        {
            _assignmentRepository = assignmentRepository;
            _submissionRepository = submissionRepository;
            _sectionRepository = sectionRepository;
            _enrollmentRepository = enrollmentRepository;
            _progressService = progressService;
            _mapper = mapper;
            _logger = logger;
        }

        // ─── Assignment CRUD ────────────────────────────────────────────────

        public async Task<IEnumerable<AssignmentResponse>> GetAssignmentsBySectionAsync(int sectionId)
        {
            var assignments = await _assignmentRepository.GetAssignmentsBySectionAsync(sectionId);
            return _mapper.Map<IEnumerable<AssignmentResponse>>(assignments);
        }

        public async Task<AssignmentResponse> GetAssignmentByIdAsync(int id)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(id);
            return _mapper.Map<AssignmentResponse>(assignment);
        }

        public async Task<AssignmentResponse> CreateAssignmentAsync(CreateAssignmentRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Assignment title cannot be empty.", nameof(request.Title));

            ValidateMarks(request.TotalMarks, request.PassingMarks);

            var assignment = _mapper.Map<Assignments>(request);

            await _assignmentRepository.AddAsync(assignment);

            _logger.LogInformation("Assignment Created: '{Title}' for SectionId={SectionId}",
                request.Title, request.CourseSectionId);

            return _mapper.Map<AssignmentResponse>(assignment);
        }

        public async Task<AssignmentResponse> UpdateAssignmentAsync(int id, UpdateAssignmentRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var assignment = await _assignmentRepository.GetByIdAsync(id);

            if (request.Title != null) assignment.Title = request.Title;
            if (request.Description != null) assignment.Description = request.Description;
            if (request.Instructions != null) assignment.Instructions = request.Instructions;
            if (request.IsCompulsory.HasValue) assignment.IsCompulsory = request.IsCompulsory.Value;
            if (request.AttachmentUrl != null) assignment.AttachmentUrl = request.AttachmentUrl;
            if (request.DurationLimitInDays.HasValue) assignment.DurationLimitInDays = request.DurationLimitInDays.Value;
            if (request.MaxSubmissions.HasValue) assignment.MaxSubmissions = request.MaxSubmissions.Value;
            if (request.IsLateSubmissionAllowed.HasValue) assignment.IsLateSubmissionAllowed = request.IsLateSubmissionAllowed.Value;

            if (request.TotalMarks.HasValue) assignment.TotalMarks = request.TotalMarks.Value;
            if (request.PassingMarks.HasValue) assignment.PassingMarks = request.PassingMarks.Value;

            ValidateMarks(assignment.TotalMarks, assignment.PassingMarks);

            await _assignmentRepository.UpdateAsync(assignment);

            _logger.LogInformation("Assignment Updated: Id={Id}", id);

            return _mapper.Map<AssignmentResponse>(assignment);
        }

        public async Task DeleteAssignmentAsync(int id)
        {
            await _assignmentRepository.DeleteAsync(id);
            _logger.LogInformation("Assignment Deleted: Id={Id}", id);
        }

        // ─── Submission Workflow ────────────────────────────────────────────

        public async Task<AssignmentSubmissionResponse> SubmitAssignmentAsync(int studentId, AssignmentSubmissionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // 1. Verify assignment exists
            var assignment = await _assignmentRepository.GetByIdAsync(request.AssignmentId);

            // 2. Verify at least one of text or URL is provided
            if (string.IsNullOrWhiteSpace(request.SubmissionText) && string.IsNullOrWhiteSpace(request.SubmittedAssignmentUrl))
                throw new ArgumentException("Submission must include either text or a file/link URL.");

            // 3. Verify student is enrolled
            var section = await _sectionRepository.GetByIdAsync(assignment.CourseSectionId);
            var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(studentId, section.CourseId);

            var isEnrolled = enrollment != null &&
                (enrollment.EnrollmentStatus == EnrollmentStatus.Active ||
                 enrollment.EnrollmentStatus == EnrollmentStatus.Completed);

            if (!isEnrolled)
                throw new UnauthorizedAccessException("Student must be enrolled in the course to submit this assignment.");

            // 4. Verify submission deadline
            if (assignment.DurationLimitInDays > 0 && enrollment != null)
            {
                var deadline = enrollment.EnrolledAt.AddDays(assignment.DurationLimitInDays);
                if (DateTime.UtcNow > deadline && !assignment.IsLateSubmissionAllowed)
                    throw new InvalidOperationException(
                        $"The submission deadline was {deadline:yyyy-MM-dd}. Late submissions are not allowed.");
            }

            // 5. Verify attempt limit using PG function
            var attemptCount = await _submissionRepository.GetSubmissionAttemptCountAsync(request.AssignmentId, studentId);
            if (attemptCount >= assignment.MaxSubmissions)
                throw new InvalidOperationException(
                    $"Maximum number of submissions ({assignment.MaxSubmissions}) has been reached for this assignment.");

            // 6. Create submission
            var submission = new AssignmentSubmissions
            {
                AssignmentId = request.AssignmentId,
                StudentId = studentId,
                SubmissionText = request.SubmissionText,
                SubmittedAssignmentUrl = request.SubmittedAssignmentUrl,
                SubmittedAt = DateTime.UtcNow,
                Status = SubmissionStatus.Submitted,
                AttemptNumber = attemptCount + 1
            };

            await _submissionRepository.AddAsync(submission);

            _logger.LogInformation("Assignment Submitted: AssignmentId={AssignmentId}, StudentId={StudentId}, Attempt={AttemptNumber}",
                request.AssignmentId, studentId, submission.AttemptNumber);

            return _mapper.Map<AssignmentSubmissionResponse>(submission);
        }

        public async Task<AssignmentSubmissionResponse> GradeAssignmentAsync(int submissionId, GradeSubmissionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Feedback))
                throw new ArgumentException("Feedback is required when grading.", nameof(request.Feedback));

            var submission = await _submissionRepository.GetByIdAsync(submissionId);
            var assignment = await _assignmentRepository.GetByIdAsync(submission.AssignmentId);

            // Validate marks range
            if (request.MarksAwarded < 0 || request.MarksAwarded > assignment.TotalMarks)
                throw new ArgumentException(
                    $"MarksAwarded must be between 0 and {assignment.TotalMarks}.");

            // Apply grade via repository (sets GradedAt, Status = Graded)
            await _submissionRepository.GradeSubmissionAsync(submissionId, request.MarksAwarded, request.Feedback);

            // Calculate pass/fail via PG function
            var isPassed = await _submissionRepository.CalculateAssignmentPassStatusAsync(submissionId);

            // Persist IsPassed
            submission = await _submissionRepository.GetByIdAsync(submissionId);
            submission.IsPassed = isPassed;
            await _submissionRepository.UpdateAsync(submission);

            if (isPassed)
                _logger.LogInformation("Assignment Passed: SubmissionId={SubmissionId}, StudentId={StudentId}",
                    submissionId, submission.StudentId);
            else
                _logger.LogInformation("Assignment Failed: SubmissionId={SubmissionId}, StudentId={StudentId}",
                    submissionId, submission.StudentId);

            _logger.LogInformation("Assignment Graded: SubmissionId={SubmissionId}, Marks={Marks}, Passed={Passed}",
                submissionId, request.MarksAwarded, isPassed);

            // Recalculate course progress so mandatory assignment affects completion
            var section = await _sectionRepository.GetByIdAsync(assignment.CourseSectionId);
            await _progressService.RecalculateCourseProgressAsync(submission.StudentId, section.CourseId);

            return _mapper.Map<AssignmentSubmissionResponse>(submission);
        }

        // ─── Queries ────────────────────────────────────────────────────────

        public async Task<AssignmentStatusResponse> GetStudentAssignmentStatusAsync(int assignmentId, int studentId)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
            var attemptCount = await _submissionRepository.GetSubmissionAttemptCountAsync(assignmentId, studentId);
            var submissions = (await _submissionRepository.GetStudentSubmissionsAsync(assignmentId, studentId)).ToList();

            var latest = submissions.FirstOrDefault(); // already ordered desc
            DateTime? deadline = null;

            if (assignment.DurationLimitInDays > 0)
            {
                var section = await _sectionRepository.GetByIdAsync(assignment.CourseSectionId);
                var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(studentId, section.CourseId);
                if (enrollment != null)
                    deadline = enrollment.EnrolledAt.AddDays(assignment.DurationLimitInDays);
            }

            return new AssignmentStatusResponse
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                AttemptsMade = attemptCount,
                MaxSubmissions = assignment.MaxSubmissions,
                RemainingAttempts = Math.Max(0, assignment.MaxSubmissions - attemptCount),
                IsPassed = latest?.IsPassed,
                LatestStatus = latest?.Status.ToString(),
                Deadline = deadline
            };
        }

        public async Task<IEnumerable<AssignmentSubmissionResponse>> GetPendingReviewsAsync(int assignmentId)
        {
            var submissions = await _submissionRepository.GetPendingSubmissionsAsync(assignmentId);
            return _mapper.Map<IEnumerable<AssignmentSubmissionResponse>>(submissions);
        }

        public async Task<IEnumerable<AssignmentSubmissionResponse>> GetStudentSubmissionsAsync(int assignmentId, int studentId)
        {
            var submissions = await _submissionRepository.GetStudentSubmissionsAsync(assignmentId, studentId);
            return _mapper.Map<IEnumerable<AssignmentSubmissionResponse>>(submissions);
        }

        // ─── Private Helpers ────────────────────────────────────────────────

        private static void ValidateMarks(int totalMarks, int passingMarks)
        {
            if (totalMarks <= 0)
                throw new ArgumentException("TotalMarks must be greater than 0.");
            if (passingMarks < 0)
                throw new ArgumentException("PassingMarks must be >= 0.");
            if (passingMarks > totalMarks)
                throw new ArgumentException($"PassingMarks ({passingMarks}) cannot exceed TotalMarks ({totalMarks}).");
        }
    }
}
