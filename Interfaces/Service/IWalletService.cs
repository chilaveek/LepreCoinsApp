using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.DTO;
namespace Interfaces.Service
{
    public interface IWalletService
    {
        Task<Result<WalletDto>> CreateWalletAsync(CreateWalletDto dto);
        Task<Result<WalletDto>> GetWalletAsync(int walletId);
        Task<Result<IEnumerable<WalletDto>>> GetUserWalletsAsync(int userId);
        Task<Result<WalletDto>> UpdateWalletBalanceAsync(int walletId, decimal amount);
        Task<Result> DeleteWalletAsync(int walletId);
    }
}
