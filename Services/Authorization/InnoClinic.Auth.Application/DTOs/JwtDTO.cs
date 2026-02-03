namespace InnoClinic.Auth.Application.DTOs
{
    public record JwtDTO(string AccessToken, string RefreshToken);
}