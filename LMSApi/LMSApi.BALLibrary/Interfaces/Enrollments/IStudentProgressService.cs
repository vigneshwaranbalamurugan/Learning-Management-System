using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IStudentProgressService
    {
        Task<LessonProgressResponse> MarkLessonCompleteAsync(int userId, int lessonId, decimal? watchPercentage = null);
        Task<CourseProgressResponse> GetCourseProgressAsync(int userId, int courseId);
        Task RecalculateCourseProgressAsync(int userId, int courseId);

        /// <summary>
        /// Saves the student's current video playback position.
        /// The backend derives the watch percentage from <paramref name="lastWatchedSecond"/> and the lesson duration.
        /// Automatically marks the lesson complete when watch percentage reaches 98%.
        /// </summary>
        Task<LessonProgressResponse> UpdateVideoProgressAsync(int userId, int lessonId, int lastWatchedSecond, int maxWatchedSecond, int totalSeconds);

        /// <summary>
        /// Returns the student's progress record for a single lesson (used to get the resume position before playback).
        /// Returns null when the student has never started this lesson.
        /// </summary>
        Task<LessonProgressResponse?> GetLessonProgressAsync(int userId, int lessonId);
        Task<InstructorCourseProgressResponse> GetStudentsProgressForCourseAsync(int instructorId, int courseId, bool isAdmin = false);
        Task<InstructorCourseAnalyticsResponse> GetCourseAnalyticsAsync(int instructorId, int courseId, bool isAdmin = false);
        Task<CourseProgressResponse> GetStudentDetailedProgressForInstructorAsync(int instructorId, int studentId, int courseId, bool isAdmin = false);
    }
}
