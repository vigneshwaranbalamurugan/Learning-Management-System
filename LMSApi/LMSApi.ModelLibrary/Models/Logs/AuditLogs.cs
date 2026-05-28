using LMSApi.ModelLibrary.Enums;
namespace LMSApi.ModelLibrary.Models
{
    public class AuditLogs
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public ActionType Action { get; set; } // e.g., "INSERT", "UPDATE", "DELETE"
        public string OldValues { get; set; } // JSON string of old values (for UPDATE and DELETE)
        public string NewValues { get; set; } // JSON string of new values (for UPDATE and INSERT)
        public DateTime Timestamp { get; set; }
        // Navigation property
        public Users User { get; set; }
    }
}