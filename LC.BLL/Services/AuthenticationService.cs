using LepreCoins.Models;
using Microsoft.EntityFrameworkCore;
using Interfaces.Service;

namespace LC.BLL.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly FamilybudgetdbContext _context;

    public AuthenticationService(FamilybudgetdbContext context)
    {
        _context = context;
    }

    public async Task<User> LoginAsync(string username, string password)
    {
        var hash = PasswordHasher.Hash(password);
        return await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Name == username &&
                u.PasswordHash == hash);
    }

    public async Task<User> RegisterAsync(User user, string password) 
    {
        if (await _context.Users.AnyAsync(u => u.Name == user.Name))
            throw new Exception("Пользователь уже существует");

        user.PasswordHash = PasswordHasher.Hash(password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
