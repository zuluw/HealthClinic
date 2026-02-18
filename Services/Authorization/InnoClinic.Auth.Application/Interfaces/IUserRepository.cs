using InnoClinic.Auth.Domain.Models;

namespace InnoClinic.Auth.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUserNameAsync(string userName, CancellationToken ct);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct);
        Task<User> GetByIdAsync(Guid id, CancellationToken ct);
        Task<bool> ExistsByUserNameAsync(string userName, CancellationToken ct);
        void Add(User user);
        Task SaveChangesAsync(CancellationToken ct);
    }
}   