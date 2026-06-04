using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IStudentProgressRepository : IRepository<int, StudentProgress>
    {
        Task<StudentProgress?> GetProgressByUserAndLessonAsync(int userId, int lessonId);
        Task<IEnumerable<StudentProgress>> GetProgressByUserAndCourseAsync(int userId, int courseId);
        Task<int> GetCompletedLessonsCountAsync(int userId, int courseId);
    }
}
