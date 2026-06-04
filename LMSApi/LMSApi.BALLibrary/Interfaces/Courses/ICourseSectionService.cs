using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ICourseSectionService
    {
        Task<IEnumerable<SectionResponse>> GetSectionsByCourseAsync(int courseId);
        Task<SectionResponse> GetSectionByIdAsync(int id);
        Task<SectionResponse> CreateSectionAsync(CreateSectionRequest request);
        Task<SectionResponse> UpdateSectionAsync(int id, UpdateSectionRequest request);
        Task DeleteSectionAsync(int id);
        Task ReorderSectionsAsync(ReorderSectionsRequest request);
    }
}
