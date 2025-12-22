using Microsoft.Extensions.DependencyInjection;
using LepreCoinsApp.Views;
namespace LepreCoinsApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            this.UserAppTheme = AppTheme.Dark;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}