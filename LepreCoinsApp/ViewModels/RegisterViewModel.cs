using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LepreCoins.Models;
using System.Windows.Input;
using LC.BLL.Session;
using LepreCoinsApp.Views;

public class RegisterViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;

    private string _userName;
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    private string _email;
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    private string _password;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public ICommand RegisterCommand { get; }
    public ICommand GoToLoginCommand { get; }

    public RegisterViewModel(IAuthenticationService authService)
    {
        _authService = authService;
        RegisterCommand = new AsyncRelayCommand(RegisterAsync);
        GoToLoginCommand = new AsyncRelayCommand(GoToLoginAsync);
    }

    private async Task RegisterAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(UserName) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка", "Заполните все поля", "OK");
                return;
            }

            var user = new User
            {
                Name = UserName,
                Email = Email
            };

            await _authService.RegisterAsync(user, Password);
            Session.CurrentUser = user;

            // Заменяем Shell на AppShell вместо навигации
            Application.Current.MainPage = new AppShell();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Ошибка", ex.Message, "OK");
        }
    }

    private async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync("//login");
    }
}
