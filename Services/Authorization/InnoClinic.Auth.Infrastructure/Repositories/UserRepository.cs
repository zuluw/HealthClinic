using InnoClinic.Auth.Application.Interfaces;
using InnoClinic.Auth.Domain.Models;
using InnoClinic.Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Auth.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> GetByUserNameAsync(string userName)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> GetByIdAsync(Guid id)
    {
        return await context.Users.FirstAsync(u => u.Id == id);
    }

    public async Task<bool> ExistsByUserNameAsync(string userName)
    {
        return await context.Users.AnyAsync(u => u.UserName == userName);
    }

    public void Add(User user)
    {
        context.Users.Add(user);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}