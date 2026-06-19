namespace LMSApi.ModelLibrary.DTOs.Logs
{
    public class AuditLogResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public int RecordId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string OldValues { get; set; } = string.Empty;
        public string NewValues { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
