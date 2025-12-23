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
        if (BindingContext is BudgetViewModel vm)
        {
            await vm.LoadBudgetDataCommand.ExecuteAsync(null);
        }
    }
}