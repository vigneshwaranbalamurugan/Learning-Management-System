using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface ILessonResourceRepository : IRepository<int, LessonResources>
    {
        Task<IEnumerable<LessonResources>> GetResourcesByLessonAsync(int lessonId);
    }
}
