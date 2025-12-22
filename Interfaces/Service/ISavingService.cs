using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.DTO;
namespace Interfaces.Service
{
    public interface ISavingService
    {
        Task<Result<SavingDto>> CreateSavingAsync(CreateSavingDto dto);
        Task<Result<SavingDto>> AddToSavingAsync(int savingId, decimal amount);
        Task<Result<SavingDto>> WithdrawFromSavingAsync(int savingId, decimal amount);
        Task<Result<IEnumerable<SavingDto>>> GetUserSavingsAsync(int userId);
        Task<Result<double>> CalculateProgressAsync(int savingId);
    }

}
