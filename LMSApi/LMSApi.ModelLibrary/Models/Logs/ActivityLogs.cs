using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class ActivityLogs
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public ActivityType ActivityType { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }

        // Navigation property
        public virtual Users User { get; set; } = null!;
    }
}