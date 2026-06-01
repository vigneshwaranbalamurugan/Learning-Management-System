using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface ICourseRepository : IRepository<int, Courses>
    {
        Task<IEnumerable<Courses>> GetCoursesByInstructorAsync(int instructorId);
        Task<IEnumerable<Courses>> GetCoursesByCategoryAsync(int categoryId);
        Task<IEnumerable<Courses>> GetPublishedCoursesAsync();
        Task<Courses?> GetCourseWithDetailsAsync(int id);
    }
}
