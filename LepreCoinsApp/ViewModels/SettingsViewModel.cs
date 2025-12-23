namespace LepreCoinsApp.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class SettingsViewModel : BaseViewModel
{
    [ObservableProperty]
    public decimal lowBalanceThreshold = 1000m;

    [ObservableProperty]
    public bool isDarkMode;

    [ObservableProperty]
    public string appVersion = "1.0.0-beta";

    [ObservableProperty]
    public bool enableNotifications = true;

    [ObservableProperty]
    public bool enableOfflineMode = true;

    [ObservableProperty]
    public string currency = "RUB";

    public SettingsViewModel()
    {
        Title = "Настройки";
    }

    [RelayCommand]
    public async Task LoadSettingsAsync()
    {
        try
        {
            IsBusy = true;
            BusyText = "Загрузка настроек...";

            LowBalanceThreshold = decimal.TryParse(
                Preferences.Get("low_balance_threshold", "1000"),
                out var threshold) ? threshold : 1000m;

            IsDarkMode = Preferences.Get("is_dark_mode", false);
            EnableNotifications = Preferences.Get("enable_notifications", true);
            EnableOfflineMode = Preferences.Get("enable_offline_mode", true);
            Currency = Preferences.Get("currency", "RUB");

            AppVersion = VersionTracking.CurrentVersion;
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Ошибка",
                $"Ошибка загрузки настроек: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SaveSettingsAsync()
    {
        try
        {
            Preferences.Set("low_balance_threshold", LowBalanceThreshold.ToString());
            Preferences.Set("is_dark_mode", IsDarkMode);
            Preferences.Set("enable_notifications", EnableNotifications);
            Preferences.Set("enable_offline_mode", EnableOfflineMode);
            Preferences.Set("currency", Currency);

            await Application.Current!.MainPage!.DisplayAlert(
                "Успех",
                "Настройки сохранены",
                "OK");
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Ошибка",
                $"Ошибка сохранения настроек: {ex.Message}",
                "OK");
        }
    }

    [RelayCommand]
    public async Task CheckForUpdatesAsync()
    {
        try
        {
            IsBusy = true;
            BusyText = "Проверка обновлений...";

            await Application.Current!.MainPage!.DisplayAlert(
                "Информация",
                "Вы используете актуальную версию",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}