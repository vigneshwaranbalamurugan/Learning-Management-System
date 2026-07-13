using LMSApi.ModelLibrary.Models;
namespace LMSApi.DALLibrary.Interfaces
{
    public interface IDiscussionRepository : IRepository<int, Discussions>
    {
        Task<IEnumerable<Discussions>> GetByLessonAsync(int lessonId);
        Task<Discussions> GetByIdWithDetailsAsync(int id);
    }
}

