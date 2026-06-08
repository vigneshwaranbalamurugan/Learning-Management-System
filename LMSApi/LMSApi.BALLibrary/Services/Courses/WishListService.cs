using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Services
{
    public class WishListService : IWishListService
    {
        private readonly IWishListRepository _wishListRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;

        public WishListService(IWishListRepository wishListRepository, IEnrollmentRepository enrollmentRepository, ICourseRepository courseRepository)
        {
            _wishListRepository = wishListRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
        }

        public async Task<WishListResponse> AddToWishListAsync(int userId, AddWishListRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null) throw new KeyNotFoundException("Course not found");

            var isEnrolled = await _enrollmentRepository.IsAlreadyEnrolledAsync(userId, request.CourseId);
            if (isEnrolled) throw new InvalidOperationException("Cannot add to wishlist because you are already enrolled in this course.");

            var exists = await _wishListRepository.CheckExistsAsync(userId, request.CourseId);
            if (exists) throw new InvalidOperationException("Course is already in your wishlist.");

            var wishList = new WishList
            {
                UserId = userId,
                CourseId = request.CourseId,
                AddedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _wishListRepository.AddAsync(wishList);

            return new WishListResponse
            {
                Id = wishList.Id,
                CourseId = wishList.CourseId,
                CourseTitle = course.Title,
                AddedAt = wishList.AddedAt
            };
        }

        public async Task RemoveFromWishListAsync(int userId, int courseId)
        {
            await _wishListRepository.RemoveAsync(userId, courseId);
        }

        public async Task<IEnumerable<WishListResponse>> GetUserWishListAsync(int userId)
        {
            var wishLists = await _wishListRepository.GetByUserAsync(userId);
            return wishLists.Select(w => new WishListResponse
            {
                Id = w.Id,
                CourseId = w.CourseId,
                CourseTitle = w.Course.Title,
                AddedAt = w.AddedAt
            });
        }
    }
}
