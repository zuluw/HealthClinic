namespace InnoClinic.Auth.Domain.Models
{
    public enum UserStatus
    {
        Unconfirmed = 1,
        Confirmed = 2,
    }

    public class User
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public UserStatus Status { get; set; } = UserStatus.Unconfirmed;
        
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}