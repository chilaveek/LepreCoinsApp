using LepreCoinsApp.ViewModels;

namespace LepreCoinsApp.Views;

public partial class WalletsPage : ContentPage
{
    private readonly WalletsViewModel _viewModel;   
    public WalletsPage(WalletsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadWalletsCommand.ExecuteAsync(null);
    }
}