using System.ComponentModel.DataAnnotations;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.DTOs
{
    // ─── Requests ────────────────────────────────────────────────────────────

    public class CreateCourseRequest
    {
        [Required(ErrorMessage = "Category ID is required.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Course title is required.")]
        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage ="Description is required.")]
        [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be zero or greater.")]
        public decimal? Price { get; set; }

        public bool IsPremium { get; set; } = false;

        [MaxLength(1000, ErrorMessage = "Requirements must not exceed 1000 characters.")]
        public string? Requirements { get; set; }
        [MaxLength(1000, ErrorMessage = "Learning outcomes must not exceed 1000 characters.")]
        public string? LearningOutcomes { get; set; }


        public CourseLevel Level { get; set; } = CourseLevel.Beginner;
        public int LanguageId { get; set; } = 1;

        // ─── Hybrid Learning ─────────────────────────────────────────────────
        public CourseAccessType CourseAccessType { get; set; } = CourseAccessType.SelfPaced;

        /// <summary>
        /// Only used when CourseAccessType = SelfPaced.
        /// Access expires this many days after enrollment. Null = never expires.
        /// </summary>
        public int? DefaultDeadlineDays { get; set; }
    }

    public class UpdateCourseRequest
    {
        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Category ID is required.")]
        public int? CategoryId { get; set; }
        [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be zero or greater.")]
        public decimal? Price { get; set; }

        public bool? IsPremium { get; set; }=false;
        [MaxLength(1000, ErrorMessage = "Requirements must not exceed 1000 characters.")]   
        public string? Requirements { get; set; }
        [MaxLength(1000, ErrorMessage = "Learning outcomes must not exceed 1000 characters.")]
        public string? LearningOutcomes { get; set; }
        public CourseLevel? Level { get; set; }=CourseLevel.Beginner;
        public int? LanguageId { get; set; }

        // ─── Hybrid Learning ─────────────────────────────────────────────────
        public CourseAccessType? CourseAccessType { get; set; }= Enums.CourseAccessType.SelfPaced;
        public int? DefaultDeadlineDays { get; set; }
    }

    // ─── Responses ───────────────────────────────────────────────────────────

    public class PublishCourseRequest
    {
        public bool Publish { get; set; }
    }

    public class ArchiveCourseRequest
    {
        public bool Archive { get; set; }
        public string? Reason { get; set; }
    }

    public class ReviewCourseRequest
    {
        [Required(ErrorMessage = "Action is required (Approve or Reject).")]
        [RegularExpression("^(Approve|Reject)$", ErrorMessage = "Action must be either 'Approve' or 'Reject'.")]
        public string Action { get; set; }

        public string? Reason { get; set; }
    }

    public class CourseResponse
    {
        public int Id { get; set; }
        public int InstructorId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public bool IsPremium { get; set; }
        public string? ThumbnailUrl { get; set; }
        public CourseLevel Level { get; set; }
        public int LanguageId { get; set; }
        public string LanguageName { get; set; }
        public CourseStatus Status { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ─── Hybrid Learning ─────────────────────────────────────────────────
        public CourseAccessType CourseAccessType { get; set; }

        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        public string InstructorName { get; set; } = string.Empty;
        public string InstructorEmail { get; set; } = string.Empty;
        public string? InstructorAvatarUrl { get; set; }
        public int LessonsCount { get; set; }
        public int EnrolledCount { get; set; }
        public double CompletionRate { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public bool HasCertificate { get; set; } = true;

        // User specific data
        public bool IsEnrolled { get; set; }
        public double EnrollmentProgress { get; set; }
        public int? EnrollmentId { get; set; }
        public IEnumerable<ReviewResponse> Reviews { get; set; } = new List<ReviewResponse>();

        public bool HasNonExpiredEnrollments { get; set; }
        public bool HasActiveEnrollments { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CourseDetailsResponse : CourseResponse
    {
        public string? IntroVideoUrl { get; set; }
        public string? Requirements { get; set; }
        public string? LearningOutcomes { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public IEnumerable<SectionResponse> Sections { get; set; } = [];

        // ─── Hybrid Learning ─────────────────────────────────────────────────
        /// <summary>Populated for CohortBased courses; empty list for SelfPaced.</summary>
        public IEnumerable<BatchSummaryResponse> AvailableBatches { get; set; } = [];

        public bool IsWishlisted { get; set; }
    }

    public class CoursePreviewResponse : CourseResponse
    {
        public string? IntroVideoUrl { get; set; }
        public string? Requirements { get; set; }
        public string? LearningOutcomes { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public IEnumerable<CourseSectionPreviewResponse> Sections { get; set; } = [];

        public IEnumerable<BatchSummaryResponse> AvailableBatches { get; set; } = [];

        public bool IsWishlisted { get; set; }
    }

    /// <summary>Used by GET /Courses/my-courses — Instructor dashboard card</summary>
    public class InstructorCourseCardResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string? ThumbnailUrl { get; set; }
        public CourseStatus Status { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int EnrolledCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public bool HasNonExpiredEnrollments { get; set; }
        public bool HasActiveEnrollments { get; set; }
        public bool IsDeleted { get; set; }
    }

    /// <summary>Used by admin/public paged listing — course table/card without heavy navigation</summary>
    public class CourseListItemResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string CategoryName { get; set; }
        public string InstructorName { get; set; }
        public string LanguageName { get; set; }
        public CourseLevel Level { get; set; }
        public CourseStatus Status { get; set; }
        public decimal? Price { get; set; }
        public bool IsPremium { get; set; }
        public CourseAccessType CourseAccessType { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int LessonsCount { get; set; }
        public int EnrolledCount { get; set; }
        public double CompletionRate { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public bool HasCertificate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CourseSummaryStatsResponse
    {
        public int TotalCourses { get; set; }
        public int PublishedCourses { get; set; }
        public int PendingApproval { get; set; }
        public int ArchivedCourses { get; set; }
    }

    public class PagedCourseResponse
    {
        public IEnumerable<CourseResponse> Courses { get; set; } = new List<CourseResponse>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class PagedInstructorCourseResponse
    {
        public IEnumerable<InstructorCourseCardResponse> Courses { get; set; } = new List<InstructorCourseCardResponse>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class PagedCourseListResponse
    {
        public IEnumerable<CourseListItemResponse> Courses { get; set; } = new List<CourseListItemResponse>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class InstructorMetadataDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class LanguageMetadataDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class FiltersMetadataResponse
    {
        public IEnumerable<CategoryResponse> Categories { get; set; } = [];
        public IEnumerable<LanguageMetadataDto> Languages { get; set; } = [];
        public IEnumerable<InstructorMetadataDto> Instructors { get; set; } = [];
    }
}
