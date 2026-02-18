namespace InnoClinic.Auth.Application.DTOs
{
    public record RefreshRequestDTO(string AccessToken, string RefreshToken);
}