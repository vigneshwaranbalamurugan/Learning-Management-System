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
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUploadService _uploadService;
        private readonly IMapper _mapper;
        private readonly ILogger<AssignmentService> _logger;
        private readonly INotificationService _notificationService;
        private readonly ICourseBatchRepository _batchRepository;
        private readonly IUserNotificationsService _userNotificationsService;

        public AssignmentService(
            IAssignmentRepository assignmentRepository,
            ICourseSectionRepository sectionRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            IUploadService uploadService,
            IMapper mapper,
            ILogger<AssignmentService> logger,
            INotificationService notificationService,
            ICourseBatchRepository batchRepository,
            IUserNotificationsService userNotificationsService)
        {
            _assignmentRepository = assignmentRepository;
            _sectionRepository = sectionRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _uploadService = uploadService;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _batchRepository = batchRepository;
            _userNotificationsService = userNotificationsService;
        }

        // ─── Assignment CRUD ────────────────────────────────────────────────

        public async Task<PagedLearnerAssignmentResponse> GetLearnerAssignmentsAsync(int userId, int pageNumber, int pageSize, string? searchQuery = null)
        {
            return await _assignmentRepository.GetLearnerAssignmentsAsync(userId, pageNumber, pageSize, searchQuery);
        }

        public async Task<IEnumerable<AssignmentResponse>> GetAssignmentsBySectionAsync(int sectionId, int? currentUserId = null, bool isAdmin = false)
        {
            var section = await _sectionRepository.GetByIdAsync(sectionId)
                ?? throw new KeyNotFoundException($"Section with id '{sectionId}' not found.");

            var course = await _courseRepository.GetByIdAsync(section.CourseId)
                ?? throw new KeyNotFoundException($"Course with id '{section.CourseId}' not found.");

            var assignments = await _assignmentRepository.GetAssignmentsBySectionAsync(sectionId);

            if (currentUserId == null || (course.InstructorId != currentUserId && !isAdmin))
            {
                assignments = assignments.Where(a => a.Status == PublishStatus.Published);
            }

            return _mapper.Map<IEnumerable<AssignmentResponse>>(assignments);
        }

        public async Task<IEnumerable<InstructorAssignmentSummaryDto>> GetInstructorAssignmentsAsync(int instructorId)
        {
            return await _assignmentRepository.GetInstructorAssignmentsAsync(instructorId);
        }

        public async Task<PagedInstructorAssignmentResponse> GetInstructorAssignmentsPagedAsync(int instructorId, int pageNumber, int pageSize, string? searchQuery, int? statusFilter)
        {
            return await _assignmentRepository.GetInstructorAssignmentsPagedAsync(instructorId, pageNumber, pageSize, searchQuery, statusFilter);
        }

        public async Task<AssignmentResponse> GetAssignmentByIdAsync(int id, int? currentUserId = null, bool isAdmin = false)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Assignment with id '{id}' not found.");

            var section = await _sectionRepository.GetByIdAsync(assignment.CourseSectionId)
                ?? throw new KeyNotFoundException($"Section with id '{assignment.CourseSectionId}' not found.");

            var course = await _courseRepository.GetByIdAsync(section.CourseId)
                ?? throw new KeyNotFoundException($"Course with id '{section.CourseId}' not found.");

            if (currentUserId == null || (course.InstructorId != currentUserId && !isAdmin))
            {
                if (assignment.Status != PublishStatus.Published)
                {
                    throw new KeyNotFoundException($"Assignment with id '{id}' not found.");
                }
            }

            return _mapper.Map<AssignmentResponse>(assignment);
        }

        public async Task<AssignmentResponse> CreateAssignmentAsync(CreateAssignmentRequest request, Stream? attachmentStream = null, string? attachmentFileName = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Assignment title cannot be empty.", nameof(request.Title));

            ValidateMarks(request.TotalMarks, request.PassingMarks);

            var section = await _sectionRepository.GetByIdAsync(request.CourseSectionId)
                ?? throw new KeyNotFoundException($"Section with id '{request.CourseSectionId}' not found.");
            var course = await _courseRepository.GetByIdAsync(section.CourseId)
                ?? throw new KeyNotFoundException($"Course with id '{section.CourseId}' not found.");

            var hasNonExpired = await _enrollmentRepository.HasNonExpiredEnrollmentsByCourseAsync(course.Id);
            if (hasNonExpired)
            {
                throw new InvalidOperationException("Cannot add an assignment to a course that has enrolled learners.");
            }

            if (course.CourseAccessType == CourseAccessType.CohortBased)
            {
                if (!request.DeadlineDate.HasValue)
                {
                    throw new ArgumentException("DeadlineDate is required for cohort-based courses.", nameof(request.DeadlineDate));
                }

                var batches = await _batchRepository.GetBatchesByCourseAsync(course.Id);
                foreach (var batch in batches)
                {
                    if (request.DeadlineDate.Value > batch.EndDate)
                    {
                        throw new ArgumentException($"DeadlineDate ({request.DeadlineDate.Value:yyyy-MM-dd}) cannot be after the Batch '{batch.Name}' end date ({batch.EndDate:yyyy-MM-dd}).");
                    }
                }
            }

            // Auto-assign SortOrder if not provided (default 0)
            if (request.SortOrder == 0)
            {
                var existingAssignments = await _assignmentRepository.GetAssignmentsBySectionAsync(request.CourseSectionId);
                request.SortOrder = existingAssignments.Any() ? existingAssignments.Max(a => a.SortOrder) + 1 : 1;
            }

            var assignment = _mapper.Map<Assignments>(request);

            if (course.CourseAccessType == CourseAccessType.CohortBased)
            {
                assignment.DeadlineInDays = 0;
                assignment.DeadlineDate = request.DeadlineDate;
            }
            else
            {
                assignment.DeadlineDate = null;
            }

            if (request.AttachmentType == AssignmentAttachmentType.File && attachmentStream != null && !string.IsNullOrWhiteSpace(attachmentFileName))
            {
                var publicId = $"assignment_{Guid.NewGuid()}";
                assignment.AttachmentUrl = await _uploadService.UploadAssignmentAttachmentAsync(
                    attachmentStream, attachmentFileName, publicId);
            }
            else if (request.AttachmentType == AssignmentAttachmentType.Link)
            {
                assignment.AttachmentUrl = request.AttachmentUrl;
            }
            else
            {
                assignment.AttachmentUrl = null;
            }

            var courseSections = await _sectionRepository.GetSectionsByCourseAsync(course.Id);
            foreach (var s in courseSections)
            {
                var existingAssignments = await _assignmentRepository.GetAssignmentsBySectionAsync(s.Id);
                if (existingAssignments.Any(a => string.Equals(a.Title.Trim(), request.Title.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException("An assignment with this title already exists in this course.");
                }
            }
            if (course.CourseAccessType == CourseAccessType.SelfPaced && course.Status == CourseStatus.Published)
            {
                assignment.Status = PublishStatus.Published;
            }

            await _assignmentRepository.AddAsync(assignment);

            _logger.LogInformation("Assignment Created: '{Title}' for SectionId={SectionId}",
                request.Title, request.CourseSectionId);

            return _mapper.Map<AssignmentResponse>(assignment);
        }

        public async Task<AssignmentResponse> UpdateAssignmentAsync(int id, UpdateAssignmentRequest request, Stream? attachmentStream = null, string? attachmentFileName = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var assignment = await _assignmentRepository.GetByIdAsync(id);

            var section = await _sectionRepository.GetByIdAsync(assignment.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            var hasNonExpired = await _enrollmentRepository.HasNonExpiredEnrollmentsByCourseAsync(course.Id);

            if (hasNonExpired)
            {
                if (request.AttachmentType.HasValue || attachmentStream != null || request.TotalMarks.HasValue || request.PassingMarks.HasValue || request.DeadlineDate.HasValue || request.DeadlineInDays.HasValue)
                {
                    throw new InvalidOperationException("Cannot update assignment files, marks, or deadlines because the course has enrolled learners.");
                }
            }

            if (request.Title != null)
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                    throw new ArgumentException("Assignment title cannot be null or empty.", nameof(request.Title));

                var courseSections = await _sectionRepository.GetSectionsByCourseAsync(course.Id);
                foreach (var s in courseSections)
                {
                    var existingAssignments = await _assignmentRepository.GetAssignmentsBySectionAsync(s.Id);
                    if (existingAssignments.Any(a => a.Id != id && string.Equals(a.Title.Trim(), request.Title.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new InvalidOperationException("An assignment with this title already exists in this course.");
                    }
                }
                assignment.Title = request.Title;
            }
            if (request.Description != null) assignment.Description = request.Description;
            if (request.Instructions != null) assignment.Instructions = request.Instructions;
            if (request.IsCompulsory.HasValue) assignment.IsCompulsory = request.IsCompulsory.Value;

            if (request.AttachmentType.HasValue)
            {
                assignment.AttachmentType = request.AttachmentType.Value;

                if (request.AttachmentType.Value == AssignmentAttachmentType.File && attachmentStream != null && !string.IsNullOrWhiteSpace(attachmentFileName))
                {
                    var publicId = $"assignment_{Guid.NewGuid()}";
                    assignment.AttachmentUrl = await _uploadService.UploadAssignmentAttachmentAsync(
                        attachmentStream, attachmentFileName, publicId);
                }
                else if (request.AttachmentType.Value == AssignmentAttachmentType.Link)
                {
                    assignment.AttachmentUrl = request.AttachmentUrl;
                }
                else if (request.AttachmentType.Value == AssignmentAttachmentType.None)
                {
                    assignment.AttachmentUrl = null;
                }
            }
            else if (request.AttachmentUrl != null) 
            {
                assignment.AttachmentUrl = request.AttachmentUrl;
            }

            if (course.CourseAccessType == CourseAccessType.CohortBased)
            {
                if (request.DeadlineInDays.HasValue && request.DeadlineInDays.Value > 0)
                {
                    throw new ArgumentException("Cohort-based assignments must use DeadlineDate instead of DeadlineInDays.");
                }

                var targetDeadlineDate = request.DeadlineDate ?? assignment.DeadlineDate;
                if (!targetDeadlineDate.HasValue)
                {
                    throw new ArgumentException("DeadlineDate is required for cohort-based courses.");
                }

                var batches = await _batchRepository.GetBatchesByCourseAsync(course.Id);
                foreach (var batch in batches)
                {
                    if (targetDeadlineDate.Value > batch.EndDate)
                    {
                        throw new ArgumentException($"DeadlineDate ({targetDeadlineDate.Value:yyyy-MM-dd}) cannot be after the Batch '{batch.Name}' end date ({batch.EndDate:yyyy-MM-dd}).");
                    }
                }

                assignment.DeadlineDate = targetDeadlineDate;
                assignment.DeadlineInDays = 0;
            }
            else
            {
                assignment.DeadlineDate = null;
                if (request.DeadlineInDays.HasValue) assignment.DeadlineInDays = request.DeadlineInDays.Value;
            }

            if (request.MaxSubmissions.HasValue) assignment.MaxSubmissions = request.MaxSubmissions.Value;
            if (request.IsLateSubmissionAllowed.HasValue) assignment.IsLateSubmissionAllowed = request.IsLateSubmissionAllowed.Value;
            
            if (request.Status.HasValue)
            {
                if (course.CourseAccessType == CourseAccessType.SelfPaced)
                {
                    throw new InvalidOperationException("Cannot manually change publish status of an assignment in a Self-Paced course.");
                }
                assignment.Status = request.Status.Value;
            }

            if (course.CourseAccessType == CourseAccessType.SelfPaced && course.Status == CourseStatus.Published)
            {
                assignment.Status = PublishStatus.Published;
            }

            if (request.SortOrder.HasValue) assignment.SortOrder = request.SortOrder.Value;

            if (request.TotalMarks.HasValue) assignment.TotalMarks = request.TotalMarks.Value;
            if (request.PassingMarks.HasValue) assignment.PassingMarks = request.PassingMarks.Value;

            ValidateMarks(assignment.TotalMarks, assignment.PassingMarks);

            await _assignmentRepository.UpdateAsync(assignment);

            _logger.LogInformation("Assignment Updated: Id={Id}", id);

            return _mapper.Map<AssignmentResponse>(assignment);
        }

        public async Task DeleteAssignmentAsync(int id)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(id);
            var section = await _sectionRepository.GetByIdAsync(assignment.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            var hasNonExpired = await _enrollmentRepository.HasNonExpiredEnrollmentsByCourseAsync(course.Id);
            if (hasNonExpired)
            {
                throw new InvalidOperationException("Cannot delete an assignment from a course that has enrolled learners.");
            }

            await _assignmentRepository.DeleteAsync(id);
            _logger.LogInformation("Assignment Deleted: Id={Id}", id);
        }

        public async Task ReorderAssignmentsAsync(ReorderAssignmentsRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.AssignmentOrders == null) throw new ArgumentException("Assignment orders list cannot be null.", nameof(request.AssignmentOrders));

            foreach (var item in request.AssignmentOrders)
            {
                var assignment = await _assignmentRepository.GetByIdAsync(item.AssignmentId);
                assignment.SortOrder = item.SortOrder;
                await _assignmentRepository.UpdateAsync(assignment);
            }

            _logger.LogInformation("Assignments Reordered: {Count} assignments updated", request.AssignmentOrders.Count);
        }

        public async Task<AssignmentResponse> PublishAssignmentAsync(int id, bool publish)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(id);
            var section = await _sectionRepository.GetByIdAsync(assignment.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            if (course.CourseAccessType == CourseAccessType.SelfPaced)
            {
                throw new InvalidOperationException("Cannot manually change publish status of an assignment in a Self-Paced course.");
            }

            assignment.Status = publish ? PublishStatus.Published : PublishStatus.Draft;
            await _assignmentRepository.UpdateAsync(assignment);

            _logger.LogInformation("Assignment Published status updated: Id={Id}, Status={Status}", id, assignment.Status);

            if (publish && course.CourseAccessType == CourseAccessType.CohortBased)
            {
                var enrollments = await _enrollmentRepository.GetActiveEnrollmentsByCourseAsync(course.Id);
                var emailsToSend = enrollments.Select(e => new
                {
                    UserId = e.UserId,
                    Email = e.User.Email,
                    Name = e.User.UserProfile?.FirstName ?? e.User.Email,
                    BatchName = e.Batch?.Name ?? ""
                }).ToList();

                var courseTitle = course.Title;
                var assignmentTitle = assignment.Title;

                _ = Task.Run(async () =>
                {
                    foreach (var e in emailsToSend)
                    {
                        var html = Utils.EmailTemplate.GetContentPublishedTemplate(
                            e.Name, courseTitle, "Assignment", assignmentTitle, e.BatchName);
                        Message msg = new EmailMessage(e.Email, $"New assignment available: {assignmentTitle}", html) { IsHtml = true };
                        try
                        {
                            await _notificationService.Send(msg);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send assignment published email to {Email}", e.Email);
                        }

                        try
                        {
                            await _userNotificationsService.CreateAndSendNotificationAsync(
                                userId: e.UserId,
                                title: "New Assignment Published",
                                message: $"A new assignment '{assignmentTitle}' is available in '{courseTitle}'.",
                                type: NotificationType.AssignmentCreated,
                                redirectUrl: $"/courses/{course.Id}/assignments/{assignment.Id}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send assignment published realtime notification to User {UserId}", e.UserId);
                        }
                    }
                });
            }

            return _mapper.Map<AssignmentResponse>(assignment);
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
