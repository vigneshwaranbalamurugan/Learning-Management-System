namespace LMSApi.ModelLibrary.Models
{
    public class DiscussionReplies
    {
        public int Id { get; set; }
        public int DiscussionId { get; set; }
        public int UserId { get; set; }
        public string ReplyText { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Discussions Discussion { get; set; }
        public Users User { get; set; }
    }
}