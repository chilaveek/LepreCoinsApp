namespace LepreCoinsApp.Views;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Регистрация маршрутов для навигации
        Routing.RegisterRoute("add-expense", typeof(MainPage));
        Routing.RegisterRoute("add-income", typeof(MainPage));
        Routing.RegisterRoute("edit-transaction", typeof(MainPage));
        Routing.RegisterRoute("settings", typeof(SettingsPage));
        Routing.RegisterRoute(nameof(BudgetCreatePage), typeof(BudgetCreatePage));
    }
}
