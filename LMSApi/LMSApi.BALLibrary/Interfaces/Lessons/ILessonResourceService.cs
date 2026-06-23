using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ILessonResourceService
    {
        Task<IEnumerable<ResourceResponse>> GetResourcesByLessonAsync(int lessonId, int? currentUserId = null, bool isAdmin = false);
        Task<ResourceResponse> GetResourceByIdAsync(int id, int? currentUserId = null, bool isAdmin = false);
        Task<ResourceResponse> AddResourceAsync(CreateResourceRequest request, System.IO.Stream? fileStream = null, string? fileName = null);
        Task<ResourceResponse> UpdateResourceAsync(int id, UpdateResourceRequest request, System.IO.Stream? fileStream = null, string? fileName = null);
        Task DeleteResourceAsync(int id);
        Task<ResourceResponse> PublishResourceAsync(int id, PublishResourceRequest request);
        Task ReorderResourcesAsync(int lessonId, ReorderResourcesRequest request);
    }
}
