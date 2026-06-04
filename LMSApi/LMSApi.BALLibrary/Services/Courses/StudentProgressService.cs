using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services
{
    public class StudentProgressService : IStudentProgressService
    {
        private readonly IStudentProgressRepository _progressRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<StudentProgressService> _logger;

        public StudentProgressService(
            IStudentProgressRepository progressRepository,
            ILessonRepository lessonRepository,
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseSectionRepository sectionRepository,
            IMapper mapper,
            ILogger<StudentProgressService> logger)
        {
            _progressRepository = progressRepository;
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _sectionRepository = sectionRepository;
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
                // If watch percentage >= 90%, mark completed automatically
                if (progress.VideoWatchedPercentage >= 90m)
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

            var totalLessons = course.Sections.SelectMany(s => s.Lessons).Count();
            var completedCount = await _progressRepository.GetCompletedLessonsCountAsync(userId, courseId);

            var progressPercent = totalLessons > 0 ? (decimal)completedCount / totalLessons * 100m : 0m;
            progressPercent = Math.Round(progressPercent, 2);

            return new CourseProgressResponse
            {
                CourseId = courseId,
                ProgressPercentage = progressPercent,
                CompletedLessonsCount = completedCount,
                TotalLessonsCount = totalLessons
            };
        }

        private async Task UpdateCourseEnrollmentProgressAsync(int userId, int courseId, Enrollments enrollment)
        {
            var course = await _courseRepository.GetCourseWithDetailsAsync(courseId);
            if (course == null) return;

            // Get total lessons count in the course
            var totalLessons = course.Sections.SelectMany(s => s.Lessons).Count();
            if (totalLessons == 0) return;

            // Get completed lessons count
            var completedCount = await _progressRepository.GetCompletedLessonsCountAsync(userId, courseId);

            var progressPercent = (decimal)completedCount / totalLessons * 100m;
            // Round to 2 decimal places to match numeric(5,2)
            progressPercent = Math.Round(progressPercent, 2);

            enrollment.ProgressPercentage = progressPercent;
            enrollment.IsCompleted = completedCount == totalLessons;
            if (enrollment.IsCompleted)
            {
                enrollment.CompletedAt ??= DateTime.UtcNow;
            }
            else
            {
                enrollment.CompletedAt = null;
            }

            await _enrollmentRepository.UpdateAsync(enrollment);
            _logger.LogInformation("Course Progress Updated: CourseId={CourseId}, StudentId={StudentId}, Progress={Progress}%", courseId, userId, progressPercent);
        }
    }
}
