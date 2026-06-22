namespace LMSApi.ModelLibrary.DTOs
{
    public class CourseSearchQuery
    {
        public string? CategoryIds { get; set; }
        public string? Levels { get; set; }
        public string? LanguageIds { get; set; }
        public bool? IsPremium { get; set; }
        public double? MinRating { get; set; }
        public string? Durations { get; set; }
        public string? InstructorIds { get; set; }
        public string? CourseAccessTypes { get; set; }
        public string? SortBy { get; set; }
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 6;
        public string? ExcludeCourseIds { get; set; }
    }
}
