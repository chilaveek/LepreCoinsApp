using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces.DTO
{
    public record WalletDto(
        int Id,
        string Name,
        decimal Balance,
        string Currency,
        int UserId);

    public record CreateWalletDto(
        string Name,
        decimal InitialBalance,
        string Currency,
        int UserId);

}
