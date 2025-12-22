namespace LepreCoinsApp.Views;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Регистрация маршрутов для навигации
        Routing.RegisterRoute("add-expense", typeof(Views.MainPage));
        Routing.RegisterRoute("add-income", typeof(Views.MainPage));
        Routing.RegisterRoute("edit-transaction", typeof(Views.MainPage));
        Routing.RegisterRoute("settings", typeof(Views.SettingsPage));
    }

    /// <summary>
    /// Обработчик нажатия на элемент меню
    /// </summary>
    private async void OnMenuItemClicked(object sender, EventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            switch (menuItem.Text)
            {
                case "Настройки":
                    await Shell.Current.GoToAsync("settings");
                    break;
                case "Выход":
                    SecureStorage.RemoveAll();
                    Preferences.Clear();
                    await Shell.Current.GoToAsync("//login");
                    break;
            }
        }
    }
}
