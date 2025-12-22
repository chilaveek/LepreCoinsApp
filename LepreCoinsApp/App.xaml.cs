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
            //var loginPage = new LoginPage(IPlatformApplication.Current.Services);
            return new Window(new AuthenticationShell());

        }
    }
}