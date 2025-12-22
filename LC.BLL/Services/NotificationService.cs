using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.Service;
using Microsoft.Extensions.Logging;
namespace LC.BLL.Services
{


    /// <summary>
    /// Сервис для отправки уведомлений пользователю
    /// Может быть расширен интеграцией с различными каналами (Email, SMS, Push-notifications)
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public async Task SendLowBalanceAlertAsync(int userId, string walletName, decimal balance, decimal threshold)
        {
            try
            {
                var message = $"ВНИМАНИЕ: Баланс кошелька '{walletName}' низкий ({balance:C})! " +
                             $"Установленный лимит уведомления: {threshold:C}";

                _logger.LogWarning($"UserId: {userId}, Message: {message}");

                // Отправить в реальных уведомлениях
                await SendNotificationAsync(userId, "Низкий баланс", message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при отправке уведомления о низком балансе: {ex.Message}");
            }
        }

        public async Task SendBudgetExceededAlertAsync(int userId, string categoryName, decimal exceeded)
        {
            try
            {
                var message = $"ВНИМАНИЕ: Бюджет категории '{categoryName}' превышен на {exceeded:C}!";

                _logger.LogWarning($"UserId: {userId}, Message: {message}");

                await SendNotificationAsync(userId, "Бюджет превышен", message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при отправке уведомления о превышении бюджета: {ex.Message}");
            }
        }

        public async Task SendSavingMilestoneAlertAsync(int userId, string savingGoal, double progressPercent)
        {
            try
            {
                var message = $"Поздравляем! Вы достигли цели сбережений '{savingGoal}'! " +
                             $"Прогресс: {progressPercent:F1}%";

                _logger.LogInformation($"UserId: {userId}, Message: {message}");

                await SendNotificationAsync(userId, "Цель достигнута", message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при отправке уведомления о достижении цели: {ex.Message}");
            }
        }

        private async Task SendNotificationAsync(int userId, string title, string message)
        {
            await Task.Delay(100); // Имитация асинхронной операции
            _logger.LogInformation($"Notification sent - Title: {title}, Message: {message}");
        }
    }
}
