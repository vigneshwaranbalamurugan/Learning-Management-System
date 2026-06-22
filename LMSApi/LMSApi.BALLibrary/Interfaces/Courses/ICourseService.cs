using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseResponse>> GetAllCoursesAsync();
        Task<PagedCourseResponse> GetPublishedCoursesPagedAsync(
            CourseSearchQuery query, int? currentUserId = null);
        Task<CourseDetailsResponse> GetCourseByIdAsync(int id, int? currentUserId = null, bool isAdmin = false);
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
        Task<CourseResponse> PublishCourseAsync(int id, PublishCourseRequest request);
        Task<IEnumerable<CourseResponse>> GetPendingCoursesAsync();
        Task<CourseResponse> ReviewCourseAsync(int id, ReviewCourseRequest request);
        Task<IEnumerable<CourseResponse>> GetCoursesByInstructorAsync(int instructorId);
        Task<IEnumerable<CourseResponse>> GetCoursesByCategoryAsync(int categoryId);
        Task<FiltersMetadataResponse> GetFiltersMetadataAsync();
        Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync();
    }
}
