using System.ComponentModel.DataAnnotations;

namespace LMSApi.ModelLibrary.DTOs
{
    public class AddWishListRequest
    {
        [Required]
        public int CourseId { get; set; }
    }

    public class WishListResponse
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
