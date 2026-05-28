namespace LMSApi.ModelLibrary.Models
{
    public class WishList
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Users User { get; set; }
        public Courses Course { get; set; }
    }
}