using System;
using System.Collections.Generic;
using System.Text;

namespace LC.BLL.Services
{
    using ApplicationCore.Interfaces;
    using Interfaces;
    using Interfaces.DTO;
    using Interfaces.Service;
    using LepreCoins.Models;

    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WalletService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<WalletDto>> CreateWalletAsync(CreateWalletDto dto)
        {
            try
            {
                var wallet = new Wallet
                {
                    Name = dto.Name,
                    Balance = dto.InitialBalance,
                    Currency = dto.Currency,
                    Userid = dto.UserId
                };

                await _unitOfWork.WalletRepository.AddAsync(wallet);
                await _unitOfWork.SaveChangesAsync();

                var walletDto = new WalletDto(
                    wallet.Id,
                    wallet.Name,
                    wallet.Balance ?? 0,
                    wallet.Currency ?? "",
                    wallet.Userid ?? 0);

                return Result<WalletDto>.Success(walletDto);
            }
            catch (Exception ex)
            {
                return Result<WalletDto>.Failure($"Ошибка при создании кошелька: {ex.Message}");
            }
        }

        public async Task<Result<WalletDto>> GetWalletAsync(int walletId)
        {
            try
            {
                var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(walletId);
                if (wallet == null)
                    return Result<WalletDto>.Failure("Кошелек не найден");

                var walletDto = new WalletDto(
                    wallet.Id,
                    wallet.Name,
                    wallet.Balance ?? 0,
                    wallet.Currency ?? "",
                    wallet.Userid ?? 0);

                return Result<WalletDto>.Success(walletDto);
            }
            catch (Exception ex)
            {
                return Result<WalletDto>.Failure($"Ошибка при получении кошелька: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<WalletDto>>> GetUserWalletsAsync(int userId)
        {
            try
            {
                var wallets = await _unitOfWork.WalletRepository.FindAsync(w => w.Userid == userId);
                var walletDtos = wallets.Select(w => new WalletDto(
                    w.Id,
                    w.Name,
                    w.Balance ?? 0,
                    w.Currency ?? "",
                    w.Userid ?? 0)).ToList();

                return Result<IEnumerable<WalletDto>>.Success(walletDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<WalletDto>>.Failure($"Ошибка при получении кошельков: {ex.Message}");
            }
        }

        public async Task<Result<WalletDto>> UpdateWalletBalanceAsync(int walletId, decimal amount)
        {
            try
            {
                var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(walletId);
                if (wallet == null)
                    return Result<WalletDto>.Failure("Кошелек не найден");

                wallet.Balance = amount;
                await _unitOfWork.WalletRepository.UpdateAsync(wallet);
                await _unitOfWork.SaveChangesAsync();

                var walletDto = new WalletDto(
                    wallet.Id,
                    wallet.Name,
                    wallet.Balance ?? 0,
                    wallet.Currency ?? "",
                    wallet.Userid ?? 0);

                return Result<WalletDto>.Success(walletDto);
            }
            catch (Exception ex)
            {
                return Result<WalletDto>.Failure($"Ошибка при обновлении баланса: {ex.Message}");
            }
        }
        public async Task<Result> DeleteWalletAsync(int walletId)
        {
            try
            {
                var wallet = await _unitOfWork.WalletRepository.GetByIdAsync(walletId);
                if (wallet == null)
                    return Result.Failure("Кошелек не найден");

                var incomes = await _unitOfWork.IncomeRepository.FindAsync(i => i.Walletid == walletId);
                foreach (var income in incomes)
                {
                    await _unitOfWork.IncomeRepository.DeleteAsync(income);
                }

        
                var expenses = await _unitOfWork.ExpenseRepository.FindAsync(e => e.Walletid == walletId);
                foreach (var expense in expenses)
                {
                    await _unitOfWork.ExpenseRepository.DeleteAsync(expense);
                }

                await _unitOfWork.WalletRepository.DeleteAsync(wallet);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Ошибка при удалении кошелька: {ex.Message}");
            }
        }


    }
}
