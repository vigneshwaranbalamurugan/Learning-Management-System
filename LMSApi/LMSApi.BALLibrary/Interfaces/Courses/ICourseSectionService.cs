using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ICourseSectionService
    {
        Task<IEnumerable<SectionResponse>> GetSectionsByCourseAsync(int courseId, int? currentUserId = null, bool isAdmin = false);
        Task<SectionResponse> GetSectionByIdAsync(int id, int? currentUserId = null, bool isAdmin = false);
        Task<SectionResponse> CreateSectionAsync(CreateSectionRequest request);
        Task<SectionResponse> UpdateSectionAsync(int id, UpdateSectionRequest request);
        Task DeleteSectionAsync(int id);
        Task ReorderSectionsAsync(ReorderSectionsRequest request);
        Task<SectionResponse> PublishSectionAsync(int id, PublishSectionRequest request);
    }
}
