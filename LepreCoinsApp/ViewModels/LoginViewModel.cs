using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Interfaces.Service;
using System.Windows.Input;
using LC.BLL.Session;
using LepreCoinsApp.Views;

public class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;

    private string _userName;
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    private string _password;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand GoToRegisterCommand { get; }

    public LoginViewModel(IAuthenticationService authService)
    {
        _authService = authService;
        LoginCommand = new AsyncRelayCommand(LoginAsync);
        GoToRegisterCommand = new AsyncRelayCommand(GoToRegisterAsync);
    }

    private async Task LoginAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка", "Заполните все поля", "OK");
                return;
            }

            var user = await _authService.LoginAsync(UserName, Password);
            if (user == null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка", "Неверный логин или пароль", "OK");
                return;
            }

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

    private async Task GoToRegisterAsync()
    {
        await Shell.Current.GoToAsync("//register");
    }
}
