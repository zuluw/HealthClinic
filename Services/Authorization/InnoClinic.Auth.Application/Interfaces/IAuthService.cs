using InnoClinic.Auth.Application.DTOs;
using InnoClinic.Auth.Domain.Models;

namespace InnoClinic.Auth.Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterDTO data);
        Task<AuthResponseDTO> LoginAsync(LoginDTO data);
        Task<AuthResponseDTO> RefreshAsync(JwtDTO data);
        Task<AuthResponseDTO> GoogleAuthAsync(string email, string firstName, string lastName);
        Task ChangeStatusAsync(Guid userId, UserStatus newStatus);
    }
}