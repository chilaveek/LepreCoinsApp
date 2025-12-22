using ApplicationCore.Interfaces;
using Interfaces.Service;
using LC.BLL.Services;
using LepreCoins.Infrastructure.LC.BLL.Services;
using LepreCoins.Infrastructure.LC.DAL;
using LepreCoins.Models;
using LepreCoinsApp;
using LepreCoinsApp.Views;
using LepreCoinsApp.ViewModels;
using Microsoft.Extensions.Logging;

namespace LepreCoinsApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Регистрация DbContext
            builder.Services.AddDbContext<FamilybudgetdbContext>();

            // Регистрация Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddScoped<IDatabaseInitializationService, DatabaseInitializationService>();

            // Регистрация сервисов приложения
            builder.Services.AddScoped<ITransactionService, TransactionService>();

            builder.Services.AddScoped<ISavingService, SavingService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IReceiptScannerService, ReceiptScannerService>();
            builder.Services.AddScoped<IWalletService, WalletService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<IBudgetService, BudgetService>();

            // Регистрация ViewModels
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddTransient<ReportsPage>();
            builder.Services.AddTransient<ReportsViewModel>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<WalletsPage>();
            builder.Services.AddTransient<WalletsViewModel>();
            builder.Services.AddTransient<BudgetViewModel>();
            builder.Services.AddTransient<BudgetPage>();

            var app = builder.Build();
            _ = InitializeDatabaseAsync(app);
            return app;
        }
        private static async Task InitializeDatabaseAsync(MauiApp app)
        {
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var dbInitializer = scope.ServiceProvider
                        .GetRequiredService<IDatabaseInitializationService>();

                    await dbInitializer.InitializeDatabaseAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка инициализации БД: {ex.Message}");
                }
            }
        }
    }
}
