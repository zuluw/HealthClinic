namespace InnoClinic.Auth.Application.DTOs
{
    public record AuthResponseDTO(
        string? Message,
        string UserName,
        string AccessToken,
        string RefreshToken
    );
}