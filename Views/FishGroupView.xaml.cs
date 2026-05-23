using ProgrammingPractice_L19.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProgrammingPractice_L19.Views
{
    /// <summary>
    /// Логика взаимодействия для FishGroupView.xaml
    /// </summary>
    public partial class FishGroupView : UserControl
    {
        public FishGroupView()
        {
            InitializeComponent();
        }

        private async void DataGridRow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is FishGroupViewModel vm)
                await vm.PrepareToUpdateAsync();
        }
    }
}
