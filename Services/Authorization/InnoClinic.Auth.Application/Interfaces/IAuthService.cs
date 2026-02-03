using InnoClinic.Auth.Application.DTOs;

namespace InnoClinic.Auth.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDTO data);
        Task<AuthResponseDTO> LoginAsync(LoginDTO data);
        Task<AuthResponseDTO> RefreshAsync(JwtDTO data);
        Task<AuthResponseDTO> GoogleAuthAsync(string email, string firstName, string lastName);
    }
}