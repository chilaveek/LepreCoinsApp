using LepreCoins.Models;

public interface IAuthenticationService
{
    Task<User?> LoginAsync(string username, string password);
    Task<User> RegisterAsync(User user, string password);
}
