using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ILessonResourceService
    {
        Task<IEnumerable<ResourceResponse>> GetResourcesByLessonAsync(int lessonId);
        Task<ResourceResponse> GetResourceByIdAsync(int id);
        Task<ResourceResponse> AddResourceAsync(CreateResourceRequest request);
        Task<ResourceResponse> UpdateResourceAsync(int id, UpdateResourceRequest request);
        Task DeleteResourceAsync(int id);
    }
}
