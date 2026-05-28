namespace LMSApi.ModelLibrary.Models
{
    public class UserRoles
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public ICollection<Users> Users { get; set; }
    }
}