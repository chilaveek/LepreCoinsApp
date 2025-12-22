using LepreCoinsApp.ViewModels;

namespace LepreCoinsApp.Views;

public partial class BudgetPage : ContentPage
{
    public BudgetPage(BudgetViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is BudgetViewModel viewModel)
        {
            if (viewModel.InitializeBudgetCommand.CanExecute(null))
            {
                await viewModel.InitializeBudgetCommand.ExecuteAsync(null);
            }
        }
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (BindingContext is BudgetViewModel viewModel)
        {
            await viewModel.RefreshStatisticsAsync();
        }
    }
}
