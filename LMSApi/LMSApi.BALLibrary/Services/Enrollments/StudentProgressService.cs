using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using LMSApi.BALLibrary.Utils;
using LMSApi.ModelLibrary.Enums;
using Microsoft.Extensions.Logging;
using Hangfire;

namespace LMSApi.BALLibrary.Services
{
    public class StudentProgressService : IStudentProgressService
    {
        private readonly IStudentProgressRepository _progressRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly IQuizAttemptRepository _quizAttemptRepository;
        private readonly IAssignmentSubmissionRepository _assignmentSubmissionRepository;
        private readonly ICertificateService _certificateService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IMapper _mapper;
        private readonly ILogger<StudentProgressService> _logger;

        public StudentProgressService(
            IStudentProgressRepository progressRepository,
            ILessonRepository lessonRepository,
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseSectionRepository sectionRepository,
            IQuizAttemptRepository quizAttemptRepository,
            IAssignmentSubmissionRepository assignmentSubmissionRepository,
            ICertificateService certificateService,
            IBackgroundJobClient backgroundJobClient,
            IMapper mapper,
            ILogger<StudentProgressService> logger)
        {
            _progressRepository = progressRepository;
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _sectionRepository = sectionRepository;
            _quizAttemptRepository = quizAttemptRepository;
            _assignmentSubmissionRepository = assignmentSubmissionRepository;
            _certificateService = certificateService;
            _backgroundJobClient = backgroundJobClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<LessonProgressResponse> MarkLessonCompleteAsync(int userId, int lessonId, decimal? watchPercentage = null)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var courseId = section.CourseId;

            // Check if enrollment exists
            var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(userId, courseId);
            if (enrollment == null)
            {
                throw new InvalidOperationException($"Student is not enrolled in course '{courseId}'.");
            }

            var progress = await _progressRepository.GetProgressByUserAndLessonAsync(userId, lessonId);
            var isNew = false;
            if (progress == null)
            {
                isNew = true;
                progress = new StudentProgress
                {
                    StudentId = userId,
                    CourseId = courseId,
                    LessonId = lessonId,
                    StartedAt = DateTime.UtcNow,
                    LastAccessed = DateTime.UtcNow
                };
            }

            progress.LastAccessed = DateTime.UtcNow;

            if (lesson.Type == LessonType.Video)
            {
                if (watchPercentage.HasValue)
                {
                    progress.VideoWatchedPercentage = watchPercentage.Value;
                }
                // If watch percentage >= 98%, mark completed automatically
                if (progress.VideoWatchedPercentage >= 98m)
                {
                    if (!progress.IsCompleted)
                    {
                        progress.IsCompleted = true;
                        progress.CompletedAt = DateTime.UtcNow;
                        _logger.LogInformation("Lesson Completed: LessonId={LessonId}, StudentId={StudentId} (via video watch percentage {Percentage}%)", lessonId, userId, progress.VideoWatchedPercentage);
                    }
                }
            }
            else
            {
                // Non-video (Pdf, Article, ExternalLink) is marked complete manually
                if (!progress.IsCompleted)
                {
                    progress.IsCompleted = true;
                    progress.CompletedAt = DateTime.UtcNow;
                    _logger.LogInformation("Lesson Completed: LessonId={LessonId}, StudentId={StudentId} (manually marked)", lessonId, userId);
                }
            }

            if (isNew)
            {
                await _progressRepository.AddAsync(progress);
            }
            else
            {
                await _progressRepository.UpdateAsync(progress);
            }

            // Recalculate Course Progress
            await UpdateCourseEnrollmentProgressAsync(userId, courseId, enrollment);

            return _mapper.Map<LessonProgressResponse>(progress);
        }

        public async Task<CourseProgressResponse> GetCourseProgressAsync(int userId, int courseId)
        {
            var course = await _courseRepository.GetCourseWithDetailsAsync(courseId);
            if (course == null) throw new KeyNotFoundException($"Course with id '{courseId}' not found.");

            // Get all published items
            var publishedLessonIds = course.Sections.SelectMany(s => s.Lessons).Where(l => l.Status == PublishStatus.Published).Select(l => l.Id).ToList();
            var publishedQuizIds = course.Sections.SelectMany(s => s.Quizzes).Where(q => q.Status == PublishStatus.Published).Select(q => q.Id).ToList();
            var publishedAssignmentIds = course.Sections.SelectMany(s => s.Assignments).Where(a => a.Status == PublishStatus.Published).Select(a => a.Id).ToList();

            var totalItems = publishedLessonIds.Count + publishedQuizIds.Count + publishedAssignmentIds.Count;

            // Fetch progress tracking data
            var allStudentProgress = await _progressRepository.GetProgressByUserAndCourseAsync(userId, courseId);
            var allQuizAttempts = publishedQuizIds.Any() ? await _quizAttemptRepository.GetAttemptsForQuizzesAsync(userId, publishedQuizIds) : new List<QuizAttempts>();
            var allSubmissions = publishedAssignmentIds.Any() ? await _assignmentSubmissionRepository.GetSubmissionsForAssignmentsAsync(userId, publishedAssignmentIds) : new List<AssignmentSubmissions>();

            int totalCompletedLessons = 0;
            int totalPassedQuizzes = 0;
            int totalPassedAssignments = 0;

            var sectionProgressList = new List<SectionProgressResponse>();

            foreach (var section in course.Sections)
            {
                var secLessons = section.Lessons.Where(l => l.Status == PublishStatus.Published).ToList();
                var secQuizzes = section.Quizzes.Where(q => q.Status == PublishStatus.Published).ToList();
                var secAssignments = section.Assignments.Where(a => a.Status == PublishStatus.Published).ToList();

                int secTotalItems = secLessons.Count + secQuizzes.Count + secAssignments.Count;
                int secCompletedItems = 0;

                // Lessons
                var lessonProgresses = new List<LessonProgressResponse>();
                foreach (var lesson in secLessons)
                {
                    var prog = allStudentProgress.FirstOrDefault(p => p.LessonId == lesson.Id);
                    bool isCompleted = prog?.IsCompleted ?? false;
                    if (isCompleted)
                    {
                        secCompletedItems++;
                        totalCompletedLessons++;
                    }
                    
                    lessonProgresses.Add(new LessonProgressResponse
                    {
                        Id = prog?.Id ?? 0,
                        UserId = userId,
                        LessonId = lesson.Id,
                        LessonTitle = lesson.Title,
                        IsCompleted = isCompleted,
                        CompletedAt = prog?.CompletedAt,
                        LastViewedAt = prog?.LastAccessed ?? DateTime.MinValue,
                        WatchPercentage = prog?.VideoWatchedPercentage ?? 0m
                    });
                }

                // Quizzes
                var quizProgresses = new List<QuizProgressResponse>();
                foreach (var quiz in secQuizzes)
                {
                    var attempts = allQuizAttempts.Where(a => a.QuizId == quiz.Id).ToList();
                    bool isPassed = attempts.Any(a => a.IsPassed);
                    if (isPassed)
                    {
                        secCompletedItems++;
                        totalPassedQuizzes++;
                    }

                    quizProgresses.Add(new QuizProgressResponse
                    {
                        QuizId = quiz.Id,
                        QuizTitle = quiz.Title,
                        IsPassed = isPassed,
                        AttemptsMade = attempts.Count
                    });
                }

                // Assignments
                var assignmentProgresses = new List<AssignmentProgressResponse>();
                foreach (var assignment in secAssignments)
                {
                    var submissions = allSubmissions.Where(s => s.AssignmentId == assignment.Id).ToList();
                    bool isPassed = submissions.Any(s => s.IsPassed == true);
                    if (isPassed)
                    {
                        secCompletedItems++;
                        totalPassedAssignments++;
                    }

                    var latestSub = submissions.OrderByDescending(s => s.SubmittedAt).FirstOrDefault();
                    assignmentProgresses.Add(new AssignmentProgressResponse
                    {
                        AssignmentId = assignment.Id,
                        AssignmentTitle = assignment.Title,
                        IsPassed = isPassed,
                        Status = latestSub?.Status.ToString() ?? "NotSubmitted"
                    });
                }

                decimal secProgressPercent = secTotalItems > 0 ? (decimal)secCompletedItems / secTotalItems * 100m : 0m;

                sectionProgressList.Add(new SectionProgressResponse
                {
                    SectionId = section.Id,
                    Title = section.Title,
                    ProgressPercentage = Math.Round(secProgressPercent, 2),
                    Lessons = lessonProgresses,
                    Quizzes = quizProgresses,
                    Assignments = assignmentProgresses
                });
            }

            var completedCount = totalCompletedLessons + totalPassedQuizzes + totalPassedAssignments;
            var progressPercent = totalItems > 0 ? (decimal)completedCount / totalItems * 100m : 0m;
            progressPercent = Math.Round(progressPercent, 2);

            return new CourseProgressResponse
            {
                CourseId = courseId,
                ProgressPercentage = progressPercent,
                CompletedLessonsCount = completedCount,
                TotalLessonsCount = totalItems,
                Sections = sectionProgressList
            };
        }

        public async Task RecalculateCourseProgressAsync(int userId, int courseId)
        {
            var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(userId, courseId);
            if (enrollment != null)
            {
                await UpdateCourseEnrollmentProgressAsync(userId, courseId, enrollment);
            }
        }

        /// <inheritdoc />
        public async Task<LessonProgressResponse> UpdateVideoProgressAsync(int userId, int lessonId, int lastWatchedSecond)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                throw new KeyNotFoundException($"Lesson with id '{lessonId}' not found.");

            if (lesson.Type != LessonType.Video)
                throw new InvalidOperationException($"Lesson '{lessonId}' is not a video lesson.");

            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var courseId = section.CourseId;

            // Verify enrollment
            var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(userId, courseId);
            if (enrollment == null)
                throw new InvalidOperationException($"Student is not enrolled in course '{courseId}'.");

            // Upsert progress record
            var progress = await _progressRepository.GetProgressByUserAndLessonAsync(userId, lessonId);
            var isNew = progress == null;
            if (isNew)
            {
                progress = new StudentProgress
                {
                    StudentId = userId,
                    CourseId = courseId,
                    LessonId = lessonId,
                    StartedAt = DateTime.UtcNow,
                    LastAccessed = DateTime.UtcNow
                };
            }

            progress!.LastWatchedSecond = lastWatchedSecond;
            progress.LastAccessed = DateTime.UtcNow;

            // Calculate watch percentage from duration stored on lesson
            if (lesson.DurationInMinutes.HasValue && lesson.DurationInMinutes.Value.TotalSeconds > 0)
            {
                var totalSeconds = (decimal)lesson.DurationInMinutes.Value.TotalSeconds;
                var rawPercentage = lastWatchedSecond / totalSeconds * 100m;
                progress.VideoWatchedPercentage = Math.Min(Math.Round(rawPercentage, 2), 100m);
            }
            // If duration is not set we cannot calculate — keep existing percentage

            // Auto-complete at 98%
            if (progress.VideoWatchedPercentage >= 98m && !progress.IsCompleted)
            {
                progress.IsCompleted = true;
                progress.CompletedAt = DateTime.UtcNow;
                _logger.LogInformation(
                    "Lesson Completed (video): LessonId={LessonId}, StudentId={StudentId}, WatchedPercentage={Percentage}%",
                    lessonId, userId, progress.VideoWatchedPercentage);
            }

            if (isNew)
                await _progressRepository.AddAsync(progress);
            else
                await _progressRepository.UpdateAsync(progress);

            // Recalculate overall course progress in the background (only when completion state changed)
            if (progress.IsCompleted)
                await UpdateCourseEnrollmentProgressAsync(userId, courseId, enrollment);

            return _mapper.Map<LessonProgressResponse>(progress);
        }

        /// <inheritdoc />
        public async Task<LessonProgressResponse?> GetLessonProgressAsync(int userId, int lessonId)
        {
            var progress = await _progressRepository.GetProgressByUserAndLessonAsync(userId, lessonId);
            if (progress == null) return null;
            return _mapper.Map<LessonProgressResponse>(progress);
        }

        private async Task UpdateCourseEnrollmentProgressAsync(int userId, int courseId, Enrollments enrollment)
        {
            var course = await _courseRepository.GetCourseWithDetailsAsync(courseId);
            if (course == null) return;

            var publishedLessonIds = course.Sections.SelectMany(s => s.Lessons).Where(l => l.Status == PublishStatus.Published).Select(l => l.Id).ToList();
            var publishedQuizIds = course.Sections.SelectMany(s => s.Quizzes).Where(q => q.Status == PublishStatus.Published).Select(q => q.Id).ToList();
            var publishedAssignmentIds = course.Sections.SelectMany(s => s.Assignments).Where(a => a.Status == PublishStatus.Published).Select(a => a.Id).ToList();

            var totalItems = publishedLessonIds.Count + publishedQuizIds.Count + publishedAssignmentIds.Count;
            if (totalItems == 0) return;

            var completedLessonsCount = await _progressRepository.GetCompletedLessonsCountAsync(userId, publishedLessonIds);
            var passedQuizzesCount = publishedQuizIds.Any() ? await _quizAttemptRepository.GetPassedQuizzesCountAsync(userId, publishedQuizIds) : 0;
            var passedAssignmentsCount = publishedAssignmentIds.Any() ? await _assignmentSubmissionRepository.GetPassedAssignmentsCountAsync(userId, publishedAssignmentIds) : 0;

            var completedCount = completedLessonsCount + passedQuizzesCount + passedAssignmentsCount;

            var progressPercent = (decimal)completedCount / totalItems * 100m;
            progressPercent = Math.Round(progressPercent, 2);

            enrollment.ProgressPercentage = progressPercent;
            enrollment.IsCompleted = completedCount == totalItems;
            if (enrollment.IsCompleted)
            {
                enrollment.CompletedAt ??= DateTime.UtcNow;
            }
            else
            {
                enrollment.CompletedAt = null;
            }

            await _enrollmentRepository.UpdateAsync(enrollment);
            _logger.LogInformation("Course Progress Recalculated: CourseId={CourseId}, StudentId={StudentId}, Progress={Progress}%", courseId, userId, progressPercent);

            if (enrollment.IsCompleted)
            {
                try
                {
                    _backgroundJobClient.Enqueue<ICertificateService>(x => x.IssueCertificateAsync(userId, courseId));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to issue certificate for User {UserId} in Course {CourseId}", userId, courseId);
                }
            }
        }

        public async Task<IEnumerable<StudentProgressSummaryDto>> GetStudentsProgressForCourseAsync(int instructorId, int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId)
                ?? throw new KeyNotFoundException($"Course with id '{courseId}' not found.");

            if (course.InstructorId != instructorId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view progress details for this course.");
            }

            var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseAsync(courseId);

            return enrollments.Select(e => new StudentProgressSummaryDto
            {
                EnrollmentId = e.Id,
                StudentId = e.UserId,
                StudentName = e.User.UserProfile != null && (!string.IsNullOrEmpty(e.User.UserProfile.FirstName) || !string.IsNullOrEmpty(e.User.UserProfile.LastName))
                    ? (e.User.UserProfile.FirstName + " " + e.User.UserProfile.LastName).Trim()
                    : MaskingUtils.MaskEmail(e.User.Email),
                StudentEmail = MaskingUtils.MaskEmail(e.User.Email),
                EnrolledAt = e.EnrolledAt,
                EnrollmentStatus = e.EnrollmentStatus,
                ProgressPercentage = e.ProgressPercentage,
                IsCompleted = e.IsCompleted,
                CompletedAt = e.CompletedAt,
                BatchName = e.Batch?.Name
            });
        }

        public async Task<CourseProgressResponse> GetStudentDetailedProgressForInstructorAsync(int instructorId, int studentId, int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId)
                ?? throw new KeyNotFoundException($"Course with id '{courseId}' not found.");

            if (course.InstructorId != instructorId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view progress details for this course.");
            }

            var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(studentId, courseId)
                ?? throw new KeyNotFoundException("Student enrollment not found in this course.");

            return await GetCourseProgressAsync(studentId, courseId);
        }
    }
}
