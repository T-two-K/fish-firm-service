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
    /// Логика взаимодействия для FishermenView.xaml
    /// </summary>
    public partial class FishermenView : UserControl
    {
        public FishermenView()
        {
            InitializeComponent();
        }

        private void DataGridRow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(DataContext is FishermenViewModel vm)
            {
                vm.PrepareToUpdate();
            }
        }
    }
}
