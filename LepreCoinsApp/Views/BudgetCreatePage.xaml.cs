using LepreCoinsApp.ViewModels;

namespace LepreCoinsApp.Views;

public partial class BudgetCreatePage : ContentPage
{
    private readonly BudgetCreateViewModel _viewModel;
    public BudgetCreatePage(BudgetCreateViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}