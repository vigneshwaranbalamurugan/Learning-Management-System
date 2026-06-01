using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseResponse>> GetAllCoursesAsync();
        Task<CourseDetailsResponse> GetCourseByIdAsync(int id);
        Task<CourseResponse> CreateCourseAsync(CreateCourseRequest request);
        Task<CourseResponse> UpdateCourseAsync(int id, UpdateCourseRequest request);
        Task DeleteCourseAsync(int id);
        Task<CourseResponse> PublishCourseAsync(int id);
        Task<CourseResponse> UnpublishCourseAsync(int id);
        Task<IEnumerable<CourseResponse>> GetCoursesByInstructorAsync(int instructorId);
        Task<IEnumerable<CourseResponse>> GetCoursesByCategoryAsync(int categoryId);
    }
}
