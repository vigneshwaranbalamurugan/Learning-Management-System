using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using System.IO;

namespace LMSApi.BALLibrary.Services
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizService> _logger;
        private readonly INotificationService _notificationService;
        private readonly ICourseBatchRepository _batchRepository;

        public QuizService(
            IQuizRepository quizRepository,
            ICourseSectionRepository sectionRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<QuizService> logger,
            INotificationService notificationService,
            ICourseBatchRepository batchRepository)
        {
            _quizRepository = quizRepository;
            _sectionRepository = sectionRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _batchRepository = batchRepository;
        }

        // ─── Quiz CRUD ──────────────────────────────────────────────────────

        public async Task<IEnumerable<QuizResponse>> GetQuizzesBySectionAsync(int sectionId, int? currentUserId = null, bool isAdmin = false)
        {
            var section = await _sectionRepository.GetByIdAsync(sectionId)
                ?? throw new KeyNotFoundException($"Section with id '{sectionId}' not found.");

            var course = await _courseRepository.GetByIdAsync(section.CourseId)
                ?? throw new KeyNotFoundException($"Course with id '{section.CourseId}' not found.");

            var quizzes = await _quizRepository.GetQuizzesBySectionAsync(sectionId);

            if (currentUserId == null || (course.InstructorId != currentUserId && !isAdmin))
            {
                quizzes = quizzes.Where(q => q.Status == PublishStatus.Published);
            }

            return _mapper.Map<IEnumerable<QuizResponse>>(quizzes);
        }

        public async Task<QuizDetailResponse> GetQuizByIdAsync(int id, int? currentUserId = null, bool isAdmin = false)
        {
            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(id)
                ?? throw new KeyNotFoundException($"Quiz with id '{id}' not found.");

            var section = await _sectionRepository.GetByIdAsync(quiz.CourseSectionId)
                ?? throw new KeyNotFoundException($"Section with id '{quiz.CourseSectionId}' not found.");

            var course = await _courseRepository.GetByIdAsync(section.CourseId)
                ?? throw new KeyNotFoundException($"Course with id '{section.CourseId}' not found.");

            if (currentUserId == null || (course.InstructorId != currentUserId && !isAdmin))
            {
                if (quiz.Status != PublishStatus.Published)
                {
                    throw new KeyNotFoundException($"Quiz with id '{id}' not found.");
                }
            }

            return _mapper.Map<QuizDetailResponse>(quiz);
        }

        public async Task<QuizResponse> CreateQuizAsync(CreateQuizRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Quiz title cannot be null or empty.", nameof(request.Title));

            // Auto-assign Order if not provided
            if (request.Order == 0)
            {
                var existing = await _quizRepository.GetQuizzesBySectionAsync(request.CourseSectionId);
                request.Order = existing.Any() ? existing.Max(q => q.Order) + 1 : 1;
            }

            var section = await _sectionRepository.GetByIdAsync(request.CourseSectionId)
                ?? throw new KeyNotFoundException($"Section with id '{request.CourseSectionId}' not found.");
            var course = await _courseRepository.GetByIdAsync(section.CourseId)
                ?? throw new KeyNotFoundException($"Course with id '{section.CourseId}' not found.");

            if (course.CourseAccessType == CourseAccessType.CohortBased)
            {
                if (!request.DeadlineDate.HasValue)
                {
                    throw new ArgumentException("DeadlineDate is required for cohort-based courses.", nameof(request.DeadlineDate));
                }

                if (request.DeadlineInDays > 0)
                {
                    throw new ArgumentException("Cohort-based quizzes must use DeadlineDate instead of DeadlineInDays.", nameof(request.DeadlineInDays));
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

            var quiz = _mapper.Map<Quzzes>(request);

            if (course.CourseAccessType == CourseAccessType.CohortBased)
            {
                quiz.DeadlineInDays = 0;
                quiz.DeadlineDate = request.DeadlineDate;
            }
            else
            {
                quiz.DeadlineDate = null;
            }

            var courseSections = await _sectionRepository.GetSectionsByCourseAsync(course.Id);
            foreach (var s in courseSections)
            {
                var existingQuizzes = await _quizRepository.GetQuizzesBySectionAsync(s.Id);
                if (existingQuizzes.Any(q => string.Equals(q.Title.Trim(), request.Title.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException("A quiz with this title already exists in this course.");
                }
            }
            
            if (course.CourseAccessType == CourseAccessType.SelfPaced && course.Status == CourseStatus.Published)
            {
                quiz.Status = PublishStatus.Published;
            }
            else
            {
                quiz.Status = PublishStatus.Draft; // defaults to unpublished
            }

            await _quizRepository.AddAsync(quiz);

            _logger.LogInformation("Quiz Created: '{Title}' for SectionId={SectionId}", request.Title, request.CourseSectionId);

            return _mapper.Map<QuizResponse>(quiz);
        }

        public async Task<QuizResponse> UpdateQuizAsync(int id, UpdateQuizRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(id);

            var section = await _sectionRepository.GetByIdAsync(quiz.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            if (request.Title != null)
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                    throw new ArgumentException("Quiz title cannot be null or empty.", nameof(request.Title));

                var courseSections = await _sectionRepository.GetSectionsByCourseAsync(course.Id);
                foreach (var s in courseSections)
                {
                    var existingQuizzes = await _quizRepository.GetQuizzesBySectionAsync(s.Id);
                    if (existingQuizzes.Any(q => q.Id != id && string.Equals(q.Title.Trim(), request.Title.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new InvalidOperationException("A quiz with this title already exists in this course.");
                    }
                }
                quiz.Title = request.Title;
            }
            
            if (course.CourseAccessType == CourseAccessType.CohortBased)
            {
                if (request.DeadlineInDays.HasValue && request.DeadlineInDays.Value > 0)
                {
                    throw new ArgumentException("Cohort-based quizzes must use DeadlineDate instead of DeadlineInDays.");
                }

                var targetDeadlineDate = request.DeadlineDate ?? quiz.DeadlineDate;
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

                quiz.DeadlineDate = targetDeadlineDate;
                quiz.DeadlineInDays = 0;
            }
            else
            {
                quiz.DeadlineDate = null;
                if (request.DeadlineInDays.HasValue) quiz.DeadlineInDays = request.DeadlineInDays.Value;
            }

            if (request.Status.HasValue)
            {
                if (course.CourseAccessType == CourseAccessType.SelfPaced)
                {
                    throw new InvalidOperationException("Cannot manually change publish status of a quiz in a Self-Paced course.");
                }
                quiz.Status = request.Status.Value;
            }

            if (course.CourseAccessType == CourseAccessType.SelfPaced && course.Status == CourseStatus.Published)
            {
                quiz.Status = PublishStatus.Published;
            }
            
            if (quiz.Status == PublishStatus.Published)
            {
                ValidateQuizMarks(quiz);
            }

            await _quizRepository.UpdateAsync(quiz);

            _logger.LogInformation("Quiz Updated: Id={Id}", id);

            return _mapper.Map<QuizResponse>(quiz);
        }

        public async Task DeleteQuizAsync(int id)
        {
            await _quizRepository.DeleteAsync(id);
            _logger.LogInformation("Quiz Deleted: Id={Id}", id);
        }

        public async Task<QuizResponse> PublishQuizAsync(int quizId, PublishQuizRequest request)
        {
            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(quizId);

            var section = await _sectionRepository.GetByIdAsync(quiz.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);
            
            if (course.CourseAccessType == CourseAccessType.SelfPaced)
            {
                throw new InvalidOperationException("Cannot manually publish or unpublish items in a Self-Paced course. Their publish status is bound to the Course.");
            }

            if (request.Publish)
            {
                if (quiz.Questions == null || !quiz.Questions.Any())
                {
                    throw new InvalidOperationException("Quiz cannot be published without at least one question.");
                }
                ValidateQuizMarks(quiz);
            }

            quiz.Status = request.Publish ? PublishStatus.Published : PublishStatus.Draft;
            await _quizRepository.UpdateAsync(quiz);

            _logger.LogInformation("Quiz Published status updated: Id={Id}, Status={Status}", quizId, quiz.Status);

            if (request.Publish && course.CourseAccessType == CourseAccessType.CohortBased)
            {
                var enrollments = await _enrollmentRepository.GetActiveEnrollmentsByCourseAsync(course.Id);
                var emailsToSend = enrollments.Select(e => new
                {
                    Email = e.User.Email,
                    Name = e.User.UserProfile?.FirstName ?? e.User.Email,
                    BatchName = e.Batch?.Name ?? ""
                }).ToList();

                var courseTitle = course.Title;
                var quizTitle = quiz.Title;

                _ = Task.Run(async () =>
                {
                    foreach (var e in emailsToSend)
                    {
                        var html = Utils.EmailTemplate.GetContentPublishedTemplate(
                            e.Name, courseTitle, "Quiz", quizTitle, e.BatchName);
                        Message msg = new EmailMessage(e.Email, $"New quiz available: {quizTitle}", html) { IsHtml = true };
                        try
                        {
                            await _notificationService.Send(msg);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send quiz published email to {Email}", e.Email);
                        }
                    }
                });
            }

            return _mapper.Map<QuizResponse>(quiz);
        }

        // ─── Private Helper validations ──────────────────────────────────────

        private void ValidateQuizMarks(Quzzes quiz)
        {
            if (quiz.PassingPercentage < 0 || quiz.PassingPercentage > 100)
            {
                throw new InvalidOperationException($"Passing percentage ({quiz.PassingPercentage}) must be between 0 and 100.");
            }
        }

    }
}