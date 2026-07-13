using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface ICourseRepository : IRepository<int, Courses>
    {
        Task<IEnumerable<Courses>> GetCoursesByInstructorAsync(int instructorId);
        Task<(IEnumerable<Courses> Courses, int TotalCount)> GetCoursesByInstructorPagedAsync(
            int instructorId, LMSApi.ModelLibrary.DTOs.CourseSearchQuery query);
        Task<IEnumerable<Courses>> GetCoursesByCategoryAsync(int categoryId);
        Task<IEnumerable<Courses>> GetPublishedCoursesAsync();
        Task<(IEnumerable<Courses> Courses, int TotalCount)> GetPublishedCoursesPagedAsync(
            LMSApi.ModelLibrary.DTOs.CourseSearchQuery query);
        Task<IEnumerable<Courses>> GetPendingCoursesAsync();
        Task<(IEnumerable<Courses> Courses, int TotalCount)> GetPendingCoursesPagedAsync(
            LMSApi.ModelLibrary.DTOs.CourseSearchQuery query);
        Task<(IEnumerable<Courses> Courses, int TotalCount)> GetUpdatesPendingCoursesPagedAsync(
            LMSApi.ModelLibrary.DTOs.CourseSearchQuery query);
        Task<(IEnumerable<Courses> Courses, int TotalCount)> GetAllCoursesPagedAsync(
            LMSApi.ModelLibrary.DTOs.CourseSearchQuery query);
        Task<Courses?> GetCourseWithDetailsAsync(int id);
        Task<Courses?> GetCourseBySlugWithDetailsAsync(string slug);
        Task<LMSApi.ModelLibrary.DTOs.CourseRatingStatsDto> GetCourseRatingStatsAsync(int courseId);
        Task<Dictionary<int, LMSApi.ModelLibrary.DTOs.CourseRatingStatsDto>> GetRatingStatsBatchAsync(IEnumerable<int> courseIds);
        Task<LMSApi.ModelLibrary.DTOs.CourseSummaryStatsResponse> GetCourseSummaryStatsAsync();
        Task<IEnumerable<CourseLanguages>> GetAllLanguagesAsync();
        Task<IEnumerable<LMSApi.ModelLibrary.DTOs.InstructorMetadataDto>> GetActiveInstructorsAsync();
        Task<IEnumerable<LMSApi.ModelLibrary.DTOs.LanguageMetadataDto>> GetActiveLanguagesAsync();
        Task UpdateCourseDurationAsync(int courseId);
        Task SoftDeleteCourseAsync(int courseId);
    }
}
