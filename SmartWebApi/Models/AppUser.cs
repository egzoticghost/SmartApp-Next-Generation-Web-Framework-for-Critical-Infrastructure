namespace SmartWebApp.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string ExternalId { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // Default
    }
}