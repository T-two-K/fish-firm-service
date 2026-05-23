using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ProgrammingPractice_L19.Database;
using ProgrammingPractice_L19.Model;
using ProgrammingPractice_L19.Service;
using System.Collections.ObjectModel;

namespace ProgrammingPractice_L19.ViewModel
{
    public partial class JarsViewModel : BaseViewModel
    {
        public ObservableCollection<Jar> Jars { get; set; }
        [ObservableProperty] private Jar? _selectedJar;

        [ObservableProperty] private bool _isUpdating = false;
        [ObservableProperty] private string _title = "Добавить новую банку";
        [ObservableProperty] private string _buttonCaption = "Добавить";

        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _coordinate = string.Empty;

        private JarsViewModel(IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service) : base(factory, dialogManager, service)
        {
            Jars = new ObservableCollection<Jar>();
        }

        static public async Task<JarsViewModel> CreateAsync(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service)
        {
            var vm = new JarsViewModel(factory, dialogManager, service);
            await vm.LoadAsync();
            return vm;
        }

        private async Task LoadAsync()
        {
            List<Jar> jars = await Service.JarRep.GetAllAsync();

            Jars = [.. jars];
        }

        public async Task StartUpdatingAsync()
        {
            if (SelectedJar == null) return;

            IsUpdating = true;
            Title = "Изменить банку";
            ButtonCaption = "Изменить";
            await FillFormAsync(SelectedJar);
        }

        [RelayCommand]
        private async Task StartAddingAsync()
        {
            IsUpdating = false;
            SelectedJar = null;
            Title = "Добавить новую банку";
            ButtonCaption = "Добавить";
            await ClearFormAsync();
        }

        [RelayCommand]
        private async Task ApplyOperation()
        {
            if (IsUpdating)
            {
                var selectedJar = SelectedJar;

                if (selectedJar == null) return;

                await ExecuteSaveAsync(async () =>
                {
                    selectedJar.Coordinate = Coordinate;
                    selectedJar.Name = Name;
                    await Service.JarRep.UpdateAsync(selectedJar);
                    int jarId = Jars.IndexOf(selectedJar);
                    Jars.RemoveAt(jarId);
                    Jars.Insert(jarId, selectedJar);
                });
            }
            else
            {
                Jar newJar = new Jar()
                {
                    Name = Name,
                    Coordinate = Coordinate,
                };

                await ExecuteSaveAsync(async () =>
                {
                    await Service.JarRep.AddAsync(newJar);
                    Jars.Add(newJar);
                    await ClearFormAsync();
                });
            }
        }

        [RelayCommand]
        private async Task Remove()
        {
            var selectedJar = SelectedJar;
            if (selectedJar == null) return;

            if (DialogManager.ShowConfirmation("Вы точно хотите удалить запись?"))
            {
                await ExecuteSaveAsync(async () =>
                {
                    await Service.JarRep.DeleteAsync(selectedJar);
                    Jars.Remove(selectedJar);
                });
            }
        }

        private async Task FillFormAsync(Jar jar)
        {
            Name = jar.Name;
            Coordinate = jar.Coordinate;
        }

        private async Task ClearFormAsync()
        {
            Name = string.Empty;
            Coordinate = string.Empty;
        }
    }
}
