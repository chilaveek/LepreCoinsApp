using LepreCoins.Models;

namespace LC.BLL.Session;

public static class Session
{
    public static User? CurrentUser { get; set; }
}
