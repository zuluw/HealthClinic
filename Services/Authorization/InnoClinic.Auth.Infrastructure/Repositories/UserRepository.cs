using InnoClinic.Auth.Application.Interfaces;
using InnoClinic.Auth.Domain.Models;
using InnoClinic.Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Auth.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken ct)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.UserName == userName, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<User> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Users.FirstAsync(u => u.Id == id, ct);
    }

    public async Task<bool> ExistsByUserNameAsync(string userName, CancellationToken ct)
    {
        return await context.Users.AnyAsync(u => u.UserName == userName, ct);
    }

    public void Add(User user)
    {
        context.Users.Add(user);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await context.SaveChangesAsync(ct);
    }
}