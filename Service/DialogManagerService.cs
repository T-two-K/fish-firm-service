using System.Windows;

namespace ProgrammingPractice_L19.Service
{
    public class DialogManagerService : IDialogManagerService
    {
        public bool ShowConfirmation(string content) =>
            MessageBox.Show(content,
                            "Предупреждение",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes;

        public void ShowErrorWindow(string content) =>
            MessageBox.Show(content,
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
    }
}
