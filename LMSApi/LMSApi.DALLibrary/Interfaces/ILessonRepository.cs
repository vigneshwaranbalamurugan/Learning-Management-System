using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface ILessonRepository : IRepository<int, Lessons>
    {
        Task<IEnumerable<Lessons>> GetLessonsBySectionAsync(int sectionId);
    }
}
