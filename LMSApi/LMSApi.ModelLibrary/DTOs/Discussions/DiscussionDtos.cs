using System.ComponentModel.DataAnnotations;
namespace LMSApi.ModelLibrary.DTOs
{
    public class CreateDiscussionRequest
    {
        [Required]
        public int LessonId { get; set; }
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
    }

    public class UpdateDiscussionRequest
    {
        [MaxLength(255)]
        public string? Title { get; set; }
        public string? Content { get; set; }
    }

    public class CreateReplyRequest
    {
        [Required]
        public string ReplyText { get; set; }
    }

    public class UpdateReplyRequest
    {
        [Required]
        public string ReplyText { get; set; }
    }

    public class ReplyResponse
    {
        public int Id { get; set; }
        public int DiscussionId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string ReplyText { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class DiscussionResponse
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int LessonId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsPinned { get; set; }
        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ReplyCount { get; set; }
        public int LikeCount { get; set; }
    }

    public class DiscussionDetailResponse : DiscussionResponse
    {
        public List<ReplyResponse> Replies { get; set; } = new List<ReplyResponse>();
    }
}
