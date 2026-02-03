using InnoClinic.Auth.Application.DTOs;

namespace InnoClinic.Auth.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDTO registerDTO);
        Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO);
        Task<AuthResponseDTO> RefreshAsync(JwtDTO jwtDTO);
        Task<AuthResponseDTO> GoogleAuthAsync(string email, string firstName, string lastName);
    }
}