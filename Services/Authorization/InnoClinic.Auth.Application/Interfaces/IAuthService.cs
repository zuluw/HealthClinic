using InnoClinic.Auth.Application.DTOs;
using InnoClinic.Auth.Domain.Models;

namespace InnoClinic.Auth.Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequestDTO data, CancellationToken ct);
        Task<AuthResponseDTO> LoginAsync(LoginRequestDTO data, CancellationToken ct);
        Task<AuthResponseDTO> RefreshAsync(RefreshRequestDTO data, CancellationToken ct);
        Task<AuthResponseDTO> GoogleAuthAsync(string email, string firstName, string lastName, CancellationToken ct);
        Task ChangeStatusAsync(Guid userId, UserStatus newStatus, CancellationToken ct);
    }
}