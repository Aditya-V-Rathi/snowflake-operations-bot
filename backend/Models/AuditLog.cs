namespace SnowflakeBot.API.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string SlackUserId { get; set; }
        public string SlackUsername { get; set; }
        public string Command { get; set; }
        public string Parameters { get; set; }
        public string Status { get; set; } // "success" or "failed"
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}