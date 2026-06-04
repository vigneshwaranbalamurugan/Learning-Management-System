using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface ILessonRepository : IRepository<int, Lessons>
    {
        Task<IEnumerable<Lessons>> GetLessonsBySectionAsync(int sectionId);

        /// <summary>Returns a lesson with its Resources collection eagerly loaded.</summary>
        Task<Lessons> GetLessonWithResourcesAsync(int lessonId);
    }
}
