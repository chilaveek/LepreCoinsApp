// MainPage.xaml.cs
namespace LepreCoinsApp.Views;

using LepreCoinsApp.ViewModels;
using LC.BLL.Session;

public partial class MainPage : ContentPage
{
    private MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        this.BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (Session.CurrentUser == null)
        {
            Application.Current.MainPage = new AppShell();
            return;
        }

        // Загружаем данные
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}
