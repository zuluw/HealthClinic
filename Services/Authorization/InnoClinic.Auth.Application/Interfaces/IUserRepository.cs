using InnoClinic.Auth.Domain.Models;

namespace InnoClinic.Auth.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUserNameAsync(string userName);
        Task<User?> GetByEmailAsync(string email);
        Task<User> GetByIdAsync(Guid id);
        Task<bool> ExistsByUserNameAsync(string userName);
        void Add(User user);
        Task SaveChangesAsync();
    }
}