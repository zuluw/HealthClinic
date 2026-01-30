using InnoClinic.Auth.Application.DTOs;

namespace InnoClinic.Auth.Application.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterDTO registerDTO);
    Task<object> LoginAsync(LoginDTO loginDTO);
    Task<object> RefreshAsync(JwtDTO jwtDTO);
    Task<object> GoogleAuthAsync(string email, string firstName, string lastName);
}