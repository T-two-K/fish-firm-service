namespace ProgrammingPractice_L19.Service
{
    public interface IDialogManagerService
    {
        public bool ShowConfirmation(string content);

        public void ShowErrorWindow(string content);
    }
}
