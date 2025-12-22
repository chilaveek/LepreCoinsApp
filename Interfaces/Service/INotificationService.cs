using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.DTO;
namespace Interfaces.Service
{
    public interface INotificationService
    {
        Task SendLowBalanceAlertAsync(int userId, string walletName, decimal balance, decimal threshold);
        Task SendBudgetExceededAlertAsync(int userId, string categoryName, decimal exceeded);
        Task SendSavingMilestoneAlertAsync(int userId, string savingGoal, double progressPercent);
    }
}
