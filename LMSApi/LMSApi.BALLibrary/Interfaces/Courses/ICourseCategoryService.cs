using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ICourseCategoryService
    {
        Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync();
        Task<CategoryResponse> GetCategoryByIdAsync(int id);
        Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);
        Task<CategoryResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
        Task DeleteCategoryAsync(int id);
    }
}
