using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IStudentProgressRepository _progressRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;

        public ReviewService(IReviewRepository reviewRepository, IStudentProgressRepository progressRepository, ICourseRepository courseRepository, IUserRepository userRepository)
        {
            _reviewRepository = reviewRepository;
            _progressRepository = progressRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
        }

        public async Task<ReviewResponse> AddReviewAsync(int userId, CreateReviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null) throw new KeyNotFoundException("Course not found.");

            // Verify progress
            var progressRecords = await _progressRepository.GetProgressByUserAndCourseAsync(userId, request.CourseId);
            if (!progressRecords.Any())
                throw new InvalidOperationException("You must start the course before you can review it.");

            var existingReview = await _reviewRepository.GetByUserAndCourseAsync(userId, request.CourseId);
            if (existingReview != null)
                throw new InvalidOperationException("You have already reviewed this course. Use update to modify your review.");

            var review = new Reviews
            {
                UserId = userId,
                CourseId = request.CourseId,
                Rating = request.Rating,
                Review = request.ReviewText,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);

            var user = await _userRepository.GetByIdAsync(userId);

            return new ReviewResponse
            {
                Id = review.Id,
                CourseId = review.CourseId,
                UserId = review.UserId,
                UserName = user.Email, // Email is used as there's no FullName in Users by default without joining UserProfiles
                Rating = review.Rating,
                ReviewText = review.Review,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };
        }

        public async Task<ReviewResponse> UpdateReviewAsync(int userId, int reviewId, UpdateReviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null || review.UserId != userId)
                throw new KeyNotFoundException("Review not found or unauthorized.");

            if (request.Rating.HasValue) review.Rating = request.Rating.Value;
            if (request.ReviewText != null) review.Review = request.ReviewText;

            review.UpdatedAt = DateTime.UtcNow;

            await _reviewRepository.UpdateAsync(review);

            return new ReviewResponse
            {
                Id = review.Id,
                CourseId = review.CourseId,
                UserId = review.UserId,
                UserName = review.User?.Email ?? "",
                Rating = review.Rating,
                ReviewText = review.Review,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };
        }

        public async Task DeleteReviewAsync(int userId, int reviewId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null || review.UserId != userId)
                throw new KeyNotFoundException("Review not found or unauthorized.");

            review.IsDeleted = true;
            await _reviewRepository.UpdateAsync(review);
        }

        public async Task DeleteReviewByAdminAsync(int reviewId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                throw new KeyNotFoundException("Review not found.");

            review.IsDeleted = true;
            await _reviewRepository.UpdateAsync(review);
        }

        public async Task<IEnumerable<ReviewResponse>> GetCourseReviewsAsync(int courseId)
        {
            var reviews = await _reviewRepository.GetByCourseAsync(courseId);
            return reviews.Select(r => new ReviewResponse
            {
                Id = r.Id,
                CourseId = r.CourseId,
                UserId = r.UserId,
                UserName = r.User?.Email ?? "",
                Rating = r.Rating,
                ReviewText = r.Review,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            });
        }

        public async Task RestoreReviewByAdminAsync(int reviewId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                throw new KeyNotFoundException("Review not found.");

            review.IsDeleted = false;
            await _reviewRepository.UpdateAsync(review);
        }

        public async Task<PagedReviewResponse> GetAllReviewsPagedAsync(int pageNumber, int pageSize, string? search, int? rating, string? status)
        {
            bool? isDeleted = null;
            if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                isDeleted = status.ToLower() == "deleted";
            }

            var (reviews, totalCount) = await _reviewRepository.GetAllReviewsPagedAsync(pageNumber, pageSize, search, rating, isDeleted);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PagedReviewResponse
            {
                Reviews = reviews.Select(r => new ReviewResponse
                {
                    Id = r.Id,
                    CourseId = r.CourseId,
                    UserId = r.UserId,
                    UserName = r.User?.Email ?? "",
                    Rating = r.Rating,
                    ReviewText = r.Review,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    CourseTitle = r.Course?.Title,
                    IsDeleted = r.IsDeleted
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }
        public async Task<PagedInstructorReviewResponse> GetInstructorReviewsPagedAsync(int instructorId, int pageNumber, int pageSize, int? ratingFilter, int? courseId, string? search)
        {
            var (reviews, totalCount, avgRating, distribution) = await _reviewRepository.GetInstructorReviewsPagedAsync(instructorId, pageNumber, pageSize, ratingFilter, courseId, search);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PagedInstructorReviewResponse
            {
                Reviews = reviews.Select(r => new InstructorReviewResponse
                {
                    Id = r.Id,
                    CourseId = r.CourseId,
                    CourseTitle = r.Course?.Title ?? "",
                    UserId = r.UserId,
                    ReviewerName = r.User?.UserProfile?.FirstName != null ? $"{r.User.UserProfile.FirstName} {r.User.UserProfile.LastName}".Trim() : r.User?.Email ?? "",
                    Rating = r.Rating,
                    ReviewText = r.Review,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                AverageRating = avgRating,
                RatingDistribution = distribution
            };
        }
    }
}
