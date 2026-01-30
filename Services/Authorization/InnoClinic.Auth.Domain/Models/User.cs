namespace InnoClinic.Auth.Domain.Models
{
    public enum UserStatus
    {
        Uncofirmed = 1,
        Confirmed = 2,
    }

    public class User
    {
        public Guid Id { get; set; } =  Guid.NewGuid();
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;   
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public UserStatus Status { get; set; } = UserStatus.Uncofirmed;
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
