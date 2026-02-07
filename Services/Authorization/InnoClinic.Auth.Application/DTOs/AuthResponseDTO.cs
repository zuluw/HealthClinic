namespace InnoClinic.Auth.Application.DTOs
{
    public record AuthResponseDTO(
        Guid UserId,
        string UserName,
        string AccessToken,
        string RefreshToken,
        string Message = ""
    );
}