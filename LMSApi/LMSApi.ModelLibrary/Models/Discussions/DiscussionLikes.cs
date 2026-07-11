namespace LMSApi.ModelLibrary.Models
{
    public class DiscussionLikes
    {
        public int Id { get; set; }
        public int DiscussionId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public Discussions Discussion { get; set; }
        public Users User { get; set; }
    }
}
