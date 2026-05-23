using ProgrammingPractice_L19.Model;
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
    /// Логика взаимодействия для VoyageView.xaml
    /// </summary>
    public partial class VoyageView : UserControl
    {
        public VoyageView()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void DataGridRow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(DataContext is VoyagesViewModel vm)
            {
                await vm.StartUpdatingAsync();
            }
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            if(sender is Border border)
            {
                border.Background = Brushes.Brown;
            }
        }

        private void ListBoxItemFishermen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(sender is ListBoxItem item
            && item.DataContext is Fisherman fisherman 
            && DataContext is VoyagesViewModel vm)
            {
                vm.RemoveFishermanCommand.Execute(fisherman);
            }
        }

        private void ListBoxItemAllFishermen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(sender is ListBoxItem item 
                && item.DataContext is Fisherman fisherman
                && DataContext is VoyagesViewModel vm)
            {
                vm.AddFishermanCommand.Execute(fisherman);
            }
        }
    }
}
