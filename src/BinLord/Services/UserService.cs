using BinLord.Data;
using BinLord.Models;
using Microsoft.EntityFrameworkCore;

namespace BinLord.Services;

public class UserService
{
    private readonly BinLordContext _context;

    public UserService(BinLordContext context)
    {
        _context = context;
    }

    public Task<bool> AnyUsersExistAsync() => _context.Users.AnyAsync();

    public Task<User?> FindByUsernameAsync(string username) =>
        _context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<User> CreateAsync(string username, string password, UserRole role)
    {
        var user = new User
        {
            Username = username,
            PasswordHash = PasswordHasher.Hash(password),
            Role = role,
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<int> CountAdminsAsync() =>
        await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
}
