using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface ICourseCategoryRepository : IRepository<int, CourseCategories>
    {
        Task<CourseCategories?> GetByNameAsync(string name);
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
    }
}
