using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ILessonResourceService
    {
        Task<IEnumerable<ResourceResponse>> GetResourcesByLessonAsync(int lessonId);
        Task<ResourceResponse> GetResourceByIdAsync(int id);
        Task<ResourceResponse> AddResourceAsync(CreateResourceRequest request, System.IO.Stream? fileStream = null, string? fileName = null);
        Task<ResourceResponse> UpdateResourceAsync(int id, UpdateResourceRequest request, System.IO.Stream? fileStream = null, string? fileName = null);
        Task DeleteResourceAsync(int id);
    }
}
