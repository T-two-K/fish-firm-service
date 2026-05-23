using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProgrammingPractice_L19.Database;
using ProgrammingPractice_L19.Service;
using ProgrammingPractice_L19.ViewModel;
using ProgrammingPractice_L19.Views;
using System.Windows;

namespace ProgrammingPractice_L19
{
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; } = null!;

        protected async override void OnStartup(StartupEventArgs e)
        {
            try
            {
                var serviceProvider = new ServiceCollection();

                IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddUserSecrets<App>()
                    .Build();

                string? connectionString = config.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Строка подключения не найдена");

                serviceProvider.AddDbContextFactory<FishFirmDbContext>(options =>
                    options.UseMySQL(connectionString));
                serviceProvider.AddSingleton<IDialogManagerService, DialogManagerService>();
                serviceProvider.AddSingleton<FishFirmService>();
                serviceProvider.AddTransient<EnterWindow>();    
                serviceProvider.AddTransient<MainViewModel>();
                serviceProvider.AddTransient<LoginViewModel>();

                ServiceProvider = serviceProvider.BuildServiceProvider();

                await using var db = await ServiceProvider
                    .GetRequiredService<IDbContextFactory<FishFirmDbContext>>().CreateDbContextAsync();

                if (await db.Database.CanConnectAsync())
                    db.Database.OpenConnection();

                var window = ServiceProvider.GetRequiredService<EnterWindow>();
                window.Show();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Ошибка!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Необработаное исключение: {ex.Message}",
                    "Ошибка!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}