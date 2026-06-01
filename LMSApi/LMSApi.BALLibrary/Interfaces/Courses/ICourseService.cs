using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseResponse>> GetAllCoursesAsync();
        Task<CourseDetailsResponse> GetCourseByIdAsync(int id);
        Task<CourseResponse> CreateCourseAsync(
            int instructorId,
            CreateCourseRequest request,
            Stream? thumbnailStream = null, string? thumbnailFileName = null,
            Stream? videoStream = null, string? videoFileName = null);
        Task<CourseResponse> UpdateCourseAsync(
            int id, UpdateCourseRequest request,
            Stream? thumbnailStream = null, string? thumbnailFileName = null,
            Stream? videoStream = null, string? videoFileName = null);
        Task DeleteCourseAsync(int id);
        Task<CourseResponse> PublishCourseAsync(int id);
        Task<CourseResponse> UnpublishCourseAsync(int id);
        Task<IEnumerable<CourseResponse>> GetCoursesByInstructorAsync(int instructorId);
        Task<IEnumerable<CourseResponse>> GetCoursesByCategoryAsync(int categoryId);
    }
}
