using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ILessonService
    {
        Task<IEnumerable<LessonResponse>> GetLessonsBySectionAsync(int sectionId, int? currentUserId = null, bool isAdmin = false);
        Task<LessonResponse> GetLessonByIdAsync(int id, int? currentUserId = null, bool isAdmin = false);

        /// <summary>Returns a lesson together with all its attached resources.</summary>
        Task<LessonDetailResponse> GetLessonDetailAsync(int id, int? currentUserId = null, bool isAdmin = false);

        Task<LessonResponse> CreateLessonAsync(CreateLessonRequest request, Stream? fileStream = null, string? fileName = null);
        Task<LessonResponse> UpdateLessonAsync(int id, UpdateLessonRequest request, Stream? fileStream = null, string? fileName = null);
        Task DeleteLessonAsync(int id);
        Task ReorderLessonsAsync(ReorderLessonsRequest request);
        Task<LessonResponse> PublishLessonAsync(int id, PublishLessonRequest request);
    }
}
