using ApplicationCore.Interfaces;
using Interfaces;
using Interfaces.DTO;
using Interfaces.Service;
using LepreCoins.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LC.BLL.Services
{
    public class SavingService : ISavingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public SavingService(IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<Result<SavingDto>> CreateSavingAsync(CreateSavingDto dto)
        {
            try
            {
                var saving = new Saving
                {
                    GoalName = dto.GoalName,
                    TargetAmount = dto.TargetAmount,
                    CurrentAmount = 0,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                    TargetDate = dto.TargetDate,
                    ProgressPercent = 0,
                    Userid = dto.UserId
                };

                await _unitOfWork.SavingRepository.AddAsync(saving);
                await _unitOfWork.SaveChangesAsync();

                var savingDto = new SavingDto(
                    saving.Id,
                    saving.GoalName ?? "",
                    saving.TargetAmount ?? 0,
                    saving.CurrentAmount ?? 0,
                    saving.CreatedDate ?? DateOnly.MinValue,
                    saving.TargetDate ?? DateOnly.MinValue,
                    saving.ProgressPercent ?? 0);

                return Result<SavingDto>.Success(savingDto);
            }
            catch (Exception ex)
            {
                return Result<SavingDto>.Failure($"Ошибка при создании цели сбережений: {ex.Message}");
            }
        }

        public async Task<Result<SavingDto>> AddToSavingAsync(int savingId, decimal amount)
        {
            try
            {
                var saving = await _unitOfWork.SavingRepository.GetByIdAsync(savingId);
                if (saving == null)
                    return Result<SavingDto>.Failure("Цель сбережений не найдена");

                saving.CurrentAmount = (saving.CurrentAmount ?? 0) + amount;
                saving.ProgressPercent = CalculateProgress(saving.CurrentAmount.Value, saving.TargetAmount ?? 0);

                // Запись о пополнении
                var transfer = new TransferToSaving
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Sum = amount,
                    Savingsid = savingId,
                    Userid = saving.Userid,
                    Isdeposit = true
                };

                await _unitOfWork.TransferToSavingRepository.AddAsync(transfer);
                await _unitOfWork.SavingRepository.UpdateAsync(saving);
                await _unitOfWork.SaveChangesAsync();

                // Проверка достижения цели
                if (saving.CurrentAmount >= saving.TargetAmount && saving.ProgressPercent >= 100)
                {
                    await _notificationService.SendSavingMilestoneAlertAsync(
                        saving.Userid ?? 0,
                        saving.GoalName ?? "",
                        saving.ProgressPercent ?? 0);
                }

                var savingDto = new SavingDto(
                    saving.Id,
                    saving.GoalName ?? "",
                    saving.TargetAmount ?? 0,
                    saving.CurrentAmount ?? 0,
                    saving.CreatedDate ?? DateOnly.MinValue,
                    saving.TargetDate ?? DateOnly.MinValue,
                    saving.ProgressPercent ?? 0);

                return Result<SavingDto>.Success(savingDto);
            }
            catch (Exception ex)
            {
                return Result<SavingDto>.Failure($"Ошибка при пополнении сбережений: {ex.Message}");
            }
        }

        public async Task<Result<SavingDto>> WithdrawFromSavingAsync(int savingId, decimal amount)
        {
            try
            {
                var saving = await _unitOfWork.SavingRepository.GetByIdAsync(savingId);
                if (saving == null)
                    return Result<SavingDto>.Failure("Цель сбережений не найдена");

                if ((saving.CurrentAmount ?? 0) < amount)
                    return Result<SavingDto>.Failure("Недостаточно средств в сбережениях");

                saving.CurrentAmount = (saving.CurrentAmount ?? 0) - amount;
                saving.ProgressPercent = CalculateProgress(saving.CurrentAmount.Value, saving.TargetAmount ?? 0);

                var transfer = new TransferToSaving
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Sum = amount,
                    Savingsid = savingId,
                    Userid = saving.Userid,
                    Isdeposit = false
                };

                await _unitOfWork.TransferToSavingRepository.AddAsync(transfer);
                await _unitOfWork.SavingRepository.UpdateAsync(saving);
                await _unitOfWork.SaveChangesAsync();

                var savingDto = new SavingDto(
                    saving.Id,
                    saving.GoalName ?? "",
                    saving.TargetAmount ?? 0,
                    saving.CurrentAmount ?? 0,
                    saving.CreatedDate ?? DateOnly.MinValue,
                    saving.TargetDate ?? DateOnly.MinValue,
                    saving.ProgressPercent ?? 0);

                return Result<SavingDto>.Success(savingDto);
            }
            catch (Exception ex)
            {
                return Result<SavingDto>.Failure($"Ошибка при снятии сбережений: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<SavingDto>>> GetUserSavingsAsync(int userId)
        {
            try
            {
                var savings = await _unitOfWork.SavingRepository.FindAsync(s => s.Userid == userId);
                var savingDtos = savings.Select(s => new SavingDto(
                    s.Id,
                    s.GoalName ?? "",
                    s.TargetAmount ?? 0,
                    s.CurrentAmount ?? 0,
                    s.CreatedDate ?? DateOnly.MinValue,
                    s.TargetDate ?? DateOnly.MinValue,
                    s.ProgressPercent ?? 0)).ToList();

                return Result<IEnumerable<SavingDto>>.Success(savingDtos);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<SavingDto>>.Failure($"Ошибка при получении сбережений: {ex.Message}");
            }
        }

        public async Task<Result<double>> CalculateProgressAsync(int savingId)
        {
            try
            {
                var saving = await _unitOfWork.SavingRepository.GetByIdAsync(savingId);
                if (saving == null)
                    return Result<double>.Failure("Цель сбережений не найдена");

                var progress = CalculateProgress(saving.CurrentAmount ?? 0, saving.TargetAmount ?? 0);
                return Result<double>.Success(progress);
            }
            catch (Exception ex)
            {
                return Result<double>.Failure($"Ошибка при расчете прогресса: {ex.Message}");
            }
        }

        private double CalculateProgress(decimal current, decimal target)
        {
            if (target <= 0) return 0;
            return (double)(current / target) * 100;
        }
    }
}
