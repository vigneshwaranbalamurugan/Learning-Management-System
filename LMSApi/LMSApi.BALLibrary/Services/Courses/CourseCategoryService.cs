using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace LMSApi.BALLibrary.Services
{
    public class CourseCategoryService : ICourseCategoryService
    {
        private const string CacheKey = "categories:all";

        private readonly ICourseCategoryRepository _categoryRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly int _ttlMinutes;
        private readonly ILogger<CourseCategoryService> _logger;

        public CourseCategoryService(
            ICourseCategoryRepository categoryRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ICacheService cacheService,
            IConfiguration configuration,
            ILogger<CourseCategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _cacheService = cacheService;
            _ttlMinutes = configuration.GetValue<int>("Cache:CategoriesTtlMinutes", 1440);
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync()
        {
            return await _cacheService.GetOrSetAsync(
                CacheKey,
                async () => 
                {
                    var categories = await _categoryRepository.GetAllAsync();
                    return _mapper.Map<IEnumerable<CategoryResponse>>(categories);
                },
                TimeSpan.FromMinutes(_ttlMinutes));
        }

        public async Task<CategoryResponse> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return _mapper.Map<CategoryResponse>(category);
        }

        public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
        {
            var isUnique = await _categoryRepository.IsNameUniqueAsync(request.Name);
            if (!isUnique)
                throw new InvalidOperationException($"A category with the name '{request.Name}' already exists.");

            var category = _mapper.Map<CourseCategories>(request);
            await _categoryRepository.AddAsync(category);

            _logger.LogInformation("Category Created: {Name}", request.Name);
            await _cacheService.InvalidateAsync(CacheKey);

            return _mapper.Map<CategoryResponse>(category);
        }

        public async Task<CategoryResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (request.Name != null && request.Name != category.Name)
            {
                var isUnique = await _categoryRepository.IsNameUniqueAsync(request.Name, excludeId: id);
                if (!isUnique)
                    throw new InvalidOperationException($"A category with the name '{request.Name}' already exists.");

                category.Name = request.Name;
            }

            if (request.Description != null)
                category.Description = request.Description;

            await _categoryRepository.UpdateAsync(category);

            _logger.LogInformation("Category Updated: Id={Id}", id);
            await _cacheService.InvalidateAsync(CacheKey);

            return _mapper.Map<CategoryResponse>(category);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            await _categoryRepository.GetByIdAsync(id); // throws KeyNotFoundException if not found

            // Business Logic: Block deletion if courses are linked
            var linkedCourses = await _courseRepository.GetCoursesByCategoryAsync(id);
            if (linkedCourses.Any())
            {
                throw new InvalidOperationException($"Category '{id}' cannot be deleted because it has {linkedCourses.Count()} assigned course(s).");
            }

            await _categoryRepository.DeleteAsync(id);

            _logger.LogInformation("Category Deleted: Id={Id}", id);
            await _cacheService.InvalidateAsync(CacheKey);
        }
    }
}
