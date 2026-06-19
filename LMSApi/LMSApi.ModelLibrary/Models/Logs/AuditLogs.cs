using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class AuditLogs
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public int RecordId { get; set; }
        public ActionType Action { get; set; }
        public string OldValues { get; set; } = string.Empty;
        public string NewValues { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }

        // Navigation property
        public virtual Users User { get; set; } = null!;
    }
}