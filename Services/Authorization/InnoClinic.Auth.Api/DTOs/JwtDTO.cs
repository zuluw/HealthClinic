namespace InnoClinic.Auth.Api.DTOs
{
    public class JwtDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
