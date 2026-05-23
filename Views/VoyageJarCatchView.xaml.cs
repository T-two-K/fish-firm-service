using ProgrammingPractice_L19.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProgrammingPractice_L19.Views
{
    /// <summary>
    /// Логика взаимодействия для CatchReportsView.xaml
    /// </summary>
    public partial class VoyageJarCatchView : UserControl
    {
        public VoyageJarCatchView()
        {
            InitializeComponent();
        }

        private async void DataGridRow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is VoyageJarCatchViewModel vm)
                await vm.StartUpdatingAsync();
        }
    }
}
