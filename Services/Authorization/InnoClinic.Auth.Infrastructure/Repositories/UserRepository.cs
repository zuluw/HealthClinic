using InnoClinic.Auth.Application.Interfaces;
using InnoClinic.Auth.Domain.Models;
using InnoClinic.Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Auth.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUserNameAsync(string userName)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<bool> ExistsByUserNameAsync(string userName)
    {
        return await _context.Users.AnyAsync(u => u.UserName == userName);
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}