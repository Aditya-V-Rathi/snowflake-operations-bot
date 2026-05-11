namespace SnowflakeBot.API.Models
{
    public class ManagedUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
