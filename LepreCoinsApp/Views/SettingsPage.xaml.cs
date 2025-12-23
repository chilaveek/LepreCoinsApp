namespace LepreCoinsApp.Views;

using LepreCoinsApp.ViewModels;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage()
    {
        InitializeComponent();

        _viewModel = new SettingsViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSettingsAsync();
    }

    private async void OnLowBalanceThresholdChanged(object sender, ValueChangedEventArgs e)
    {
        var newThreshold = (decimal)e.NewValue;

        Preferences.Set("low_balance_threshold", newThreshold.ToString());

        await DisplayAlert("??????????", $"????? ??????????: {newThreshold:C}", "OK");
    }

    private async void OnClearCacheClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "?????????????",
            "???????? ??? ???????????",
            "??",
            "???");

        if (confirm)
        {
            try
            {
                // TODO: ??????? ?????? ??????? ????
                await DisplayAlert("?????", "??? ??????", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("??????", ex.Message, "OK");
            }
        }
    }
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "?????????????",
            "?? ???????, ??? ?????? ??????",
            "??",
            "???");

        if (confirm)
        {
            SecureStorage.RemoveAll();
            Preferences.Clear();

            await Shell.Current.GoToAsync("//login");
        }
    }
    private void OnThemeToggled(object sender, ToggledEventArgs e)
    {
        // ???????? ???? ??????????
        var isDarkMode = e.Value;

        Preferences.Set("is_dark_mode", isDarkMode);
    }
}