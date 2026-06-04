using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ILessonService
    {
        Task<IEnumerable<LessonResponse>> GetLessonsBySectionAsync(int sectionId);
        Task<LessonResponse> GetLessonByIdAsync(int id);

        /// <summary>Returns a lesson together with all its attached resources.</summary>
        Task<LessonDetailResponse> GetLessonDetailAsync(int id);

        Task<LessonResponse> CreateLessonAsync(CreateLessonRequest request, Stream? fileStream = null, string? fileName = null);
        Task<LessonResponse> UpdateLessonAsync(int id, UpdateLessonRequest request, Stream? fileStream = null, string? fileName = null);
        Task DeleteLessonAsync(int id);
        Task ReorderLessonsAsync(ReorderLessonsRequest request);
    }
}
