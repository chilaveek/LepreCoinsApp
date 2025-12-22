using ApplicationCore.Interfaces;
using Interfaces.Service;
using LC.BLL.Services;
using LepreCoins.Infrastructure.LC.BLL.Services;
using LepreCoins.Infrastructure.LC.DAL;
using LepreCoins.Models;
using LepreCoinsApp;
using LepreCoinsApp.ViewModels;
using LepreCoinsApp.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration.Json;

using Npgsql;

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
            //var connectionString = "Server=localhost;Port=5432;Database=familybudgetdb_phantom;User Id=postgres;Password=12345;";
            builder.Configuration.AddJsonFile("appsettings.json",optional: false,reloadOnChange: true);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<FamilybudgetdbContext>(options =>
                options.UseNpgsql(connectionString));


            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IDatabaseInitializationService, DatabaseInitializationService>();
            builder.Services.AddScoped<ITransactionService, TransactionService>();
            builder.Services.AddScoped<ISavingService, SavingService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IReceiptScannerService, ReceiptScannerService>();
            builder.Services.AddScoped<IWalletService, WalletService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<IBudgetService, BudgetService>();

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

            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<RegisterPage>();
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<RegisterViewModel>();
            builder.Services.AddSingleton<AuthenticationShell>();

            var app = builder.Build();

            Task.Run(async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Начинаю инициализацию БД...");

                    await CreateDatabaseIfNotExistsAsync(connectionString);

                    await Task.Delay(1500);

                    using var scope = app.Services.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<FamilybudgetdbContext>();

                    await dbContext.Database.OpenConnectionAsync();
                    await dbContext.Database.EnsureCreatedAsync();
                    await dbContext.Database.CloseConnectionAsync();

                    await dbContext.Database.EnsureCreatedAsync();

                    var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseInitializationService>();
                    await databaseService.InitializeDatabaseAsync();

                }
                catch (NpgsqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка PostgreSQL: {ex.Message}");

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        try
                        {
                            await Task.Delay(500);
                            if (Application.Current?.MainPage != null)
                            {
                                await Application.Current.MainPage.DisplayAlertAsync(
                                    "Ошибка подключения к БД",
                                    "Не удалось подключиться к PostgreSQL.\n\n" +
                                    $"Детали: {ex.Message}",
                                    "OK");
                            }
                        }
                        catch (Exception dialogEx)
                        {
                        }
                    });
                }
                catch (Exception ex)
                {

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        try
                        {
                            await Task.Delay(500);
                            if (Application.Current?.MainPage != null)
                            {
                                await Application.Current.MainPage.DisplayAlertAsync(
                                    "Ошибка инициализации БД",
                                    $"Произошла ошибка:\n\n{ex.Message}",
                                    "OK");
                            }
                        }
                        catch (Exception dialogEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ Ошибка диалога: {dialogEx.Message}");
                        }
                    });
                }
            });




            return app;
        }
        private static async Task CreateDatabaseIfNotExistsAsync(string connectionString)
        {
            try
            {
                var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
                var databaseName = connectionStringBuilder.Database;

                System.Diagnostics.Debug.WriteLine($"Проверяю БД: '{databaseName}'");

                connectionStringBuilder.Database = "postgres";
                var adminConnectionString = connectionStringBuilder.ConnectionString;

                using var connection = new NpgsqlConnection(adminConnectionString);
                await connection.OpenAsync();

                System.Diagnostics.Debug.WriteLine($"Подключение к PostgreSQL установлено");

                // ПРОВЕРЯЕМ СУЩЕСТВУЕТ ЛИ БД
                using (var cmd = new NpgsqlCommand(
                    $"SELECT 1 FROM pg_database WHERE datname = @dbname",
                    connection))
                {
                    cmd.Parameters.AddWithValue("@dbname", databaseName);
                    var exists = await cmd.ExecuteScalarAsync();

                    if (exists == null)
                    {

                        using var createCmd = new NpgsqlCommand(
                            $"CREATE DATABASE \"{databaseName}\" " +
                            $"ENCODING 'UTF8' " +
                            $"LC_COLLATE 'en_US.UTF-8' " +
                            $"LC_CTYPE 'en_US.UTF-8' " +
                            $"TEMPLATE template0;",
                            connection);

                        await createCmd.ExecuteNonQueryAsync();
                    }
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка при создании БД: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
