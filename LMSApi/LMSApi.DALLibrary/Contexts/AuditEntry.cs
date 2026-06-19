using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace LMSApi.DALLibrary.Contexts
{
    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }
        public int UserId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public int RecordId { get; set; }
        public ActionType Action { get; set; }
        public Dictionary<string, object> OldValues { get; } = new();
        public Dictionary<string, object> NewValues { get; } = new();
        public List<PropertyEntry> TemporaryProperties { get; } = new();

        public bool HasTemporaryProperties => TemporaryProperties.Any();

        public AuditLogs ToAuditLog()
        {
            return new AuditLogs
            {
                UserId = UserId,
                TableName = TableName,
                RecordId = RecordId,
                Action = Action,
                OldValues = OldValues.Any() ? JsonSerializer.Serialize(OldValues) : "{}",
                NewValues = NewValues.Any() ? JsonSerializer.Serialize(NewValues) : "{}",
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
