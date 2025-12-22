namespace LepreCoinsApp.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
    }
}
