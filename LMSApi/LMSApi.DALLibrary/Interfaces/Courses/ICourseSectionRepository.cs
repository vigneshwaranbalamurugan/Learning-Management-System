using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface ICourseSectionRepository : IRepository<int, CourseSection>
    {
        Task<IEnumerable<CourseSection>> GetSectionsByCourseAsync(int courseId);
    }
}
