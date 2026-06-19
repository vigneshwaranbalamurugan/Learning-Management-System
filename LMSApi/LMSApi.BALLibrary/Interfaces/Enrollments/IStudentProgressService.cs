using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IStudentProgressService
    {
        Task<LessonProgressResponse> MarkLessonCompleteAsync(int userId, int lessonId, decimal? watchPercentage = null);
        Task<CourseProgressResponse> GetCourseProgressAsync(int userId, int courseId);
        Task RecalculateCourseProgressAsync(int userId, int courseId);
    }
}
