namespace LMSApi.ModelLibrary.DTOs.Logs
{
    public class ActivityLogResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
