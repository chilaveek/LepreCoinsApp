using LepreCoinsApp.ViewModels;

namespace LepreCoinsApp.Views;

public partial class ReportsPage : ContentPage
{
    private ReportsViewModel _viewModel;

    public ReportsPage(ReportsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.IsReportVisible)
        {
        }
    }
}
