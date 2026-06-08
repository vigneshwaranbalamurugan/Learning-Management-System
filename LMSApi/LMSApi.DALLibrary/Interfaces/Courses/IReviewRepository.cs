using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IReviewRepository : IRepository<int, Reviews>
    {
        Task<Reviews> GetByUserAndCourseAsync(int userId, int courseId);
        Task<IEnumerable<Reviews>> GetByCourseAsync(int courseId);
    }
}
