using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Interfaces.DTO;
using Interfaces.Service;
using LepreCoins.Models;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Collections.ObjectModel;
using System.Globalization;

namespace LepreCoinsApp.ViewModels
{
    public partial class WalletsViewModel : BaseViewModel
    {
        private readonly IWalletService _walletService;
        private readonly int _currentUserId;
        private readonly IAuthenticationService _authService;

        [ObservableProperty]
        public ObservableCollection<WalletDto> wallets = new();

        [ObservableProperty]
        public WalletDto? selectedWallet;

        [ObservableProperty]
        public bool isAddWalletPopupVisible;

        [ObservableProperty]
        public string walletNameText = string.Empty;

        [ObservableProperty]
        public string walletInitialBalanceText = string.Empty;

        [ObservableProperty]
        public string walletCurrency = "RUB";

        [ObservableProperty]
        public string walletFormError = string.Empty;

        [ObservableProperty]
        public decimal totalBalance;

        public bool HasWalletFormError => !string.IsNullOrWhiteSpace(WalletFormError);

        partial void OnWalletFormErrorChanged(string value) =>
            OnPropertyChanged(nameof(HasWalletFormError));

        public WalletsViewModel(IWalletService walletService,
    IAuthenticationService authService)
        {
            _currentUserId = authService.GetCurrentUserId();
            _walletService = walletService;  
            _authService = authService;        
            Title = "Кошельки";
        }


        [RelayCommand]
        public async Task LoadWalletsAsync()
        {
            try
            {

                var result = await _walletService.GetUserWalletsAsync(_currentUserId);

                if (result.IsSuccess && result.Data != null)
                {
                    Wallets = new ObservableCollection<WalletDto>(result.Data);
                    CalculateTotalBalance();
                }
                else
                {
                    WalletFormError = result.ErrorMessage ?? "Ошибка при загрузке кошельков";
                }
            }
            catch (Exception ex)
            {
                WalletFormError = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                BusyText = "";
            }
        }

        [RelayCommand]
        public Task AddWalletAsync()
        {
            WalletNameText = "";
            WalletInitialBalanceText = "";
            WalletCurrency = "RUB";
            WalletFormError = "";
            IsAddWalletPopupVisible = true;
            return Task.CompletedTask;
        }

        [RelayCommand]
        private void CancelAddWallet()
        {
            IsAddWalletPopupVisible = false;
            WalletFormError = "";
        }

        [RelayCommand]
        public async Task SaveWalletAsync()
        {
            WalletFormError = "";

            // Валидация имени
            var name = (WalletNameText ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                WalletFormError = "Введите название кошелька.";
                return;
            }

            var normalized = (WalletInitialBalanceText ?? "").Trim().Replace(",", ".");
            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var balance))
            {
                WalletFormError = "Введите корректный начальный баланс.";
                return;
            }

            if (balance < 0)
            {
                WalletFormError = "Баланс не может быть отрицательным.";
                return;
            }

            IsBusy = true;
            BusyText = "Создание кошелька...";

            try
            {
                var dto = new CreateWalletDto(
                    Name: name,
                    InitialBalance: balance,
                    Currency: WalletCurrency,
                    UserId: _currentUserId);

                var result = await _walletService.CreateWalletAsync(dto);

                if (!result.IsSuccess)
                {
                    WalletFormError = result.ErrorMessage;
                    return;
                }

                // Перезагрузить список кошельков
                await LoadWalletsAsync();
                IsAddWalletPopupVisible = false;
            }
            catch (Exception ex)
            {
                WalletFormError = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                BusyText = "";
            }
        }
        [RelayCommand]
        public async Task DeleteWalletAsync(WalletDto wallet)
        {
            if (wallet == null)
                return;

            var confirmed = await Application.Current!.MainPage!.DisplayAlert(
                "Подтверждение",
                $"Вы действительно хотите удалить кошелек \"{wallet.Name}\"?",
                "Да",
                "Нет");

            if (!confirmed)
                return;

            BusyText = "Удаление кошелька...";

            try
            {
                var result = await _walletService.DeleteWalletAsync(wallet.Id);
                if (!result.IsSuccess)
                {
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Ошибка",
                        result.ErrorMessage,
                        "OK");
                    return;
                }

                Wallets.Remove(wallet);
                CalculateTotalBalance();

                await Application.Current!.MainPage!.DisplayAlert(
                    "Успех",
                    $"Кошелек \"{wallet.Name}\" удален",
                    "OK");
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Ошибка",
                    $"Ошибка при удалении: {ex.Message} {ex.InnerException}",
                    "OK");
            }
            finally
            {
                IsBusy = false;
                BusyText = "";
            }
        }

        [RelayCommand]
        public async Task UpdateWalletBalanceAsync(WalletDto wallet)
        {
            if (wallet == null)
                return;

            var newBalanceStr = await Application.Current!.MainPage!.DisplayPromptAsync(
                "Обновить баланс",
                $"Текущий баланс: {wallet.Balance:C}",
                "OK",
                "Отмена",
                placeholder: wallet.Balance.ToString("F2"));

            if (string.IsNullOrEmpty(newBalanceStr))
                return;

            var normalized = newBalanceStr.Replace(",", ".");
            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var newBalance))
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Ошибка",
                    "Введите корректную сумму",
                    "OK");
                return;
            }

            IsBusy = true;
            BusyText = "Обновление баланса...";

            try
            {
                var result = await _walletService.UpdateWalletBalanceAsync(wallet.Id, newBalance);

                if (result.IsSuccess)
                {
                    await LoadWalletsAsync();
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Успех",
                        "Баланс обновлен",
                        "OK");
                }
                else
                {
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Ошибка",
                        result.ErrorMessage,
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Ошибка",
                    $"Ошибка: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsBusy = false;
                BusyText = "";
            }
        }

        private void CalculateTotalBalance()
        {
            TotalBalance = Wallets.Sum(w => w.Balance);
        }
    }
}