using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseResponse>> GetAllCoursesAsync();
        Task<PagedCourseListResponse> GetAllCoursesPagedAsync(CourseSearchQuery query);
        Task<PagedCourseListResponse> GetPublishedCoursesPagedAsync(
            CourseSearchQuery query, int? currentUserId = null);
        Task<CourseResponse> GetCourseByIdAsync(int id, int? currentUserId = null, bool isAdmin = false);
        Task<CourseResponse> GetCourseBySlugAsync(string slug, int? currentUserId = null, bool isAdmin = false);
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
        Task SoftDeleteCourseAsync(int id);
        Task<CourseResponse> PublishCourseAsync(int id, PublishCourseRequest request);
        Task<CourseResponse> ArchiveCourseAsync(int id, ArchiveCourseRequest request);
        Task<IEnumerable<CourseResponse>> GetPendingCoursesAsync();
        Task<PagedCourseListResponse> GetPendingCoursesPagedAsync(CourseSearchQuery query);
        Task<CourseResponse> ReviewCourseAsync(int id, ReviewCourseRequest request);
        Task<IEnumerable<CourseResponse>> GetCoursesByInstructorAsync(int instructorId);
        Task<PagedInstructorCourseResponse> GetCoursesByInstructorPagedAsync(int instructorId, CourseSearchQuery query);
        Task<IEnumerable<CourseResponse>> GetCoursesByCategoryAsync(int categoryId);
        Task<CourseSummaryStatsResponse> GetCourseSummaryStatsAsync();
        Task<FiltersMetadataResponse> GetFiltersMetadataAsync();
        Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync();
    }
}
