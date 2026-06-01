using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ILessonService
    {
        Task<IEnumerable<LessonResponse>> GetLessonsBySectionAsync(int sectionId);
        Task<LessonResponse> GetLessonByIdAsync(int id);
        Task<LessonResponse> CreateLessonAsync(CreateLessonRequest request);
        Task<LessonResponse> UpdateLessonAsync(int id, UpdateLessonRequest request);
        Task DeleteLessonAsync(int id);
        Task ReorderLessonsAsync(ReorderLessonsRequest request);
    }
}
