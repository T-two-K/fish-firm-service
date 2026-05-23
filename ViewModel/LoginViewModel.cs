using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ProgrammingPractice_L19.Database;
using ProgrammingPractice_L19.Model;
using ProgrammingPractice_L19.Service;
using ProgrammingPractice_L19.Views;

namespace ProgrammingPractice_L19.ViewModel
{
    public partial class LoginViewModel : BaseViewModel
    {

        private IServiceProvider _serviceProvider;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _textBoxLogin = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _textBoxPassword = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public LoginViewModel(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service,
            IServiceProvider serviceProvider) : base(factory, dialogManager, service)
        {
            _serviceProvider = serviceProvider;
        }

        [RelayCommand(CanExecute = (nameof(CanLogin)))]
        private async Task Login()
        {
            await using var DatabaseService = await Factory.CreateDbContextAsync();

            if (DatabaseService.Users.Any(u => u.Login == TextBoxLogin))
            {
                User? user = await DatabaseService.Users.FirstOrDefaultAsync(u => u.Login == TextBoxLogin);
                if (user != null && user.Password == TextBoxPassword)
                {
                    MainWindow mainWindow = user.Role switch
                    {
                        "admin" => new MainWindow(new MainViewModel(Factory, DialogManager, Service, true)),
                        "manager" => new MainWindow(new MainViewModel(Factory, DialogManager, Service, false)),
                        _ => throw new Exception("Такой роли нет")
                    };

                    mainWindow.Show();
                    return;
                }
            }

            ErrorMessage = "Нет такого пользователя или пароля";
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(TextBoxPassword)
                && !string.IsNullOrWhiteSpace(TextBoxLogin);
        }
    }
}
