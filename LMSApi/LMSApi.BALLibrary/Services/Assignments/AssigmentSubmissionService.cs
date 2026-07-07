using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.DALLibrary.Interfaces;
using Microsoft.Extensions.Logging;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Services
{
    public class AssignmentSubmissionService : IAssignmentSubmissionService
    {

        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IAssignmentSubmissionRepository _submissionRepository;
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IStudentProgressService _progressService;
        private readonly IUploadService _uploadService;
        private readonly IMapper _mapper;
        private readonly ILogger<AssignmentSubmissionService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IUserNotificationsService _userNotificationsService;
        private readonly IUserRepository _userRepository;

        public AssignmentSubmissionService(
            IAssignmentRepository assignmentRepository,
            IAssignmentSubmissionRepository submissionRepository,
            ICourseSectionRepository sectionRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            IStudentProgressService progressService,
            IUploadService uploadService,
            IMapper mapper,
            ILogger<AssignmentSubmissionService> logger,
            INotificationService notificationService,
            IUserNotificationsService userNotificationsService,
            IUserRepository userRepository)        {
            _assignmentRepository = assignmentRepository;
            _submissionRepository = submissionRepository;
            _sectionRepository = sectionRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _progressService = progressService;
            _uploadService = uploadService;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _userNotificationsService = userNotificationsService;
            _userRepository = userRepository;
        }

        // ─── Submission Workflow ────────────────────────────────────────────

        public async Task<AssignmentSubmissionResponse> SubmitAssignmentAsync(int studentId, AssignmentSubmissionRequest request, Stream? attachmentStream = null, string? attachmentFileName = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // 1. Verify assignment exists
            var assignment = await _assignmentRepository.GetByIdAsync(request.AssignmentId);

            // 2. Verify at least one of text or URL or file is provided
            if (string.IsNullOrWhiteSpace(request.SubmissionText) && string.IsNullOrWhiteSpace(request.SubmittedAssignmentUrl) && attachmentStream == null)
                throw new ArgumentException("Submission must include either text, a file, or a link URL.");

            // 3. Verify student is enrolled
            var section = await _sectionRepository.GetByIdAsync(assignment.CourseSectionId);
            var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(studentId, section.CourseId);
            var isEnrolled = enrollment != null &&
                (enrollment.EnrollmentStatus == EnrollmentStatus.Active ||
                 enrollment.EnrollmentStatus == EnrollmentStatus.Completed);

            if (!isEnrolled)
                throw new UnauthorizedAccessException("Student must be enrolled in the course to submit this assignment.");

            var course = await _courseRepository.GetByIdAsync(section.CourseId)
                ?? throw new KeyNotFoundException($"Course with id '{section.CourseId}' not found.");

            // 4. Verify submission deadline
            if (course.CourseAccessType == CourseAccessType.CohortBased)
            {
                if (assignment.DeadlineDate.HasValue)
                {
                    if (DateTime.UtcNow > assignment.DeadlineDate.Value && !assignment.IsLateSubmissionAllowed)
                        throw new InvalidOperationException(
                            $"The submission deadline was {assignment.DeadlineDate.Value:yyyy-MM-dd HH:mm:ss}. Late submissions are not allowed.");
                }
            }
            else
            {
                if (assignment.DeadlineInDays > 0)
                {
                    if (enrollment != null)
                    {
                        var deadline = enrollment.EnrolledAt.AddDays(assignment.DeadlineInDays);
                        if (DateTime.UtcNow > deadline && !assignment.IsLateSubmissionAllowed)
                            throw new InvalidOperationException(
                                $"The submission deadline was {deadline:yyyy-MM-dd}. Late submissions are not allowed.");
                    }
                }
            }

            // 5. Verify attempt limit using PG function
            var attemptCount = await _submissionRepository.GetSubmissionAttemptCountAsync(request.AssignmentId, studentId);
            if (attemptCount >= assignment.MaxSubmissions)
                throw new InvalidOperationException(
                    $"Maximum number of submissions ({assignment.MaxSubmissions}) has been reached for this assignment.");

            // 5b. Verify existing submissions status
            var previousSubmissions = await _submissionRepository.GetStudentSubmissionsAsync(request.AssignmentId, studentId);
            if (previousSubmissions.Any(s => s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.UnderReview))
            {
                throw new InvalidOperationException("You already have a submission that is pending review. You cannot submit another attempt until it is graded.");
            }
            if (previousSubmissions.Any(s => s.IsPassed == true))
            {
                throw new InvalidOperationException("You have already passed this assignment and cannot submit another attempt.");
            }

            if(request.AttachmentType == AssignmentSubmissonAttachmentType.File && attachmentStream == null)
                throw new ArgumentException("Attachment file must be provided when AttachmentType is File.");
            if(request.AttachmentType==AssignmentSubmissonAttachmentType.Link && request.SubmittedAssignmentUrl==null)
                throw new ArgumentNullException("Attachment link must be provided when AttachmentType is Link ");
            // 6. Create submission
            var submission = new AssignmentSubmissions
            {
                AssignmentId = request.AssignmentId,
                StudentId = studentId,
                SubmissionText = request.SubmissionText,
                AttachmentType = request.AttachmentType,
                SubmittedAt = DateTime.UtcNow,
                Status = SubmissionStatus.Submitted,
                AttemptNumber = attemptCount + 1
            };

            if (request.AttachmentType == AssignmentSubmissonAttachmentType.File && attachmentStream != null && !string.IsNullOrWhiteSpace(attachmentFileName))
            {
                var publicId = $"submission_{Guid.NewGuid()}";
                submission.SubmittedAssignmentUrl = await _uploadService.UploadAssignmentAttachmentAsync(
                    attachmentStream, attachmentFileName, publicId);
            }
            else if (request.AttachmentType == AssignmentSubmissonAttachmentType.Link)
            {
                submission.SubmittedAssignmentUrl = request.SubmittedAssignmentUrl;
            }
            else
            {
                submission.SubmittedAssignmentUrl = request.SubmittedAssignmentUrl;
            }

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
            
            // Avoid duplicate grade processing
            if (submission.Status == SubmissionStatus.Graded && 
                submission.MarksAwarded == request.MarksAwarded && 
                string.Equals(submission.Feedback, request.Feedback, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The submission has already been graded with the exact same marks and feedback.");
            }

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

            // ── Send Assignment Graded Email (fire-and-forget) ──
            var student = await _userRepository.GetByIdAsync(submission.StudentId);
            var studentName = student.UserProfile?.FirstName ?? student.Email;
            var course = await _courseRepository.GetByIdAsync(section.CourseId);
            var gradeHtml = Utils.EmailTemplate.GetAssignmentGradedTemplate(
                studentName, assignment.Title,
                request.MarksAwarded, assignment.TotalMarks, isPassed, request.Feedback);
            
            Message gradeMsg = new EmailMessage(student.Email, $"Assignment Graded: {assignment.Title}", gradeHtml) { IsHtml = true };
            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.Send(gradeMsg);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send assignment graded email to {Email}", student.Email);
                }
            });

            // ── Send real-time SignalR notification to the student ──
            var resultText = isPassed ? "Passed ✅" : "Failed ❌";
            var marks = $"{request.MarksAwarded}/{assignment.TotalMarks}";
            try
            {
                await _userNotificationsService.CreateAndSendNotificationAsync(
                    userId: submission.StudentId,
                    title: $"Assignment Graded: {assignment.Title}",
                    message: $"You scored {marks} — {resultText}. Feedback: {request.Feedback}",
                    type: NotificationType.AssignmentGraded);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send real-time grading notification to Student {StudentId}", submission.StudentId);
            }

            return _mapper.Map<AssignmentSubmissionResponse>(submission);
        }

                // ─── Queries ────────────────────────────────────────────────────────

        public async Task<AssignmentStatusResponse> GetStudentAssignmentStatusAsync(int assignmentId, int studentId)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
            var attemptCount = await _submissionRepository.GetSubmissionAttemptCountAsync(assignmentId, studentId);
            var submissions = (await _submissionRepository.GetStudentSubmissionsAsync(assignmentId, studentId)).ToList();

            var latest = submissions.FirstOrDefault(); // already ordered desc
            var section = await _sectionRepository.GetByIdAsync(assignment.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);
            DateTime? deadline = null;

            if (course.CourseAccessType == CourseAccessType.CohortBased)
            {
                deadline = assignment.DeadlineDate;
            }
            else if (assignment.DeadlineInDays > 0)
            {
                var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(studentId, section.CourseId);
                if (enrollment != null)
                    deadline = enrollment.EnrolledAt.AddDays(assignment.DeadlineInDays);
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
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
            var section = await _sectionRepository.GetByIdAsync(assignment.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            var responseList = new List<AssignmentSubmissionResponse>();

            foreach (var sub in submissions)
            {
                var res = _mapper.Map<AssignmentSubmissionResponse>(sub);

                DateTime? studentDeadline = null;
                if (course.CourseAccessType == CourseAccessType.CohortBased)
                {
                    studentDeadline = assignment.DeadlineDate;
                }
                else if (assignment.DeadlineInDays > 0)
                {
                    var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(sub.StudentId, section.CourseId);
                    if (enrollment != null)
                    {
                        studentDeadline = enrollment.EnrolledAt.AddDays(assignment.DeadlineInDays);
                    }
                }

                res.StudentDeadline = studentDeadline;
                res.IsLate = studentDeadline.HasValue && sub.SubmittedAt > studentDeadline.Value;

                responseList.Add(res);
            }

            return responseList;
        }

        public async Task<IEnumerable<AssignmentSubmissionResponse>> GetGradedReviewsAsync(int assignmentId)
        {
            var submissions = await _submissionRepository.GetGradedSubmissionsAsync(assignmentId);
            var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
            var section = await _sectionRepository.GetByIdAsync(assignment.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            var responseList = new List<AssignmentSubmissionResponse>();

            foreach (var sub in submissions)
            {
                var res = _mapper.Map<AssignmentSubmissionResponse>(sub);

                DateTime? studentDeadline = null;
                if (course.CourseAccessType == CourseAccessType.CohortBased)
                {
                    studentDeadline = assignment.DeadlineDate;
                }
                else if (assignment.DeadlineInDays > 0)
                {
                    var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(sub.StudentId, section.CourseId);
                    if (enrollment != null)
                    {
                        studentDeadline = enrollment.EnrolledAt.AddDays(assignment.DeadlineInDays);
                    }
                }

                res.StudentDeadline = studentDeadline;
                res.IsLate = studentDeadline.HasValue && sub.SubmittedAt > studentDeadline.Value;

                responseList.Add(res);
            }

            return responseList;
        }

        public async Task<IEnumerable<AssignmentSubmissionResponse>> GetStudentSubmissionsAsync(int assignmentId, int studentId)
        {
            var submissions = await _submissionRepository.GetStudentSubmissionsAsync(assignmentId, studentId);
            return _mapper.Map<IEnumerable<AssignmentSubmissionResponse>>(submissions);
        }

        public async Task<PagedAssignmentSubmissionResponse> GetAllSubmissionsPagedAsync(int pageNumber, int pageSize, string? status, string? search = null)
        {
            var (submissions, totalCount) = await _submissionRepository.GetAllSubmissionsPagedAsync(pageNumber, pageSize, status, search);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PagedAssignmentSubmissionResponse
            {
                Submissions = _mapper.Map<IEnumerable<AssignmentSubmissionResponse>>(submissions),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

    }
}