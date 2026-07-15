using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface ILessonAiSummaryRepository
    {
        Task<LessonAiSummary?> GetByLessonIdAsync(int lessonId);
        Task UpsertAsync(LessonAiSummary summary);
        Task DeleteByLessonIdAsync(int lessonId);
    }
}
