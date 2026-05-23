using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ProgrammingPractice_L19.Database;
using ProgrammingPractice_L19.Model;
using ProgrammingPractice_L19.Service;
using System.Collections.ObjectModel;

namespace ProgrammingPractice_L19.ViewModel
{
    public partial class VoyageJarViewModel : BaseViewModel
    {
        public ObservableCollection<VoyageJar> VoyageJars { get; set; }
        [ObservableProperty] private VoyageJar? _selectedVoyageJar;

        public ObservableCollection<Voyage> Voyages { get; set; }
        [ObservableProperty] private Voyage? _selectedVoyage;

        public ObservableCollection<Jar> Jars { get; set; }
        [ObservableProperty] private Jar? _selectedJar;

        [ObservableProperty] private DateTime _datePickerBoatArrive = DateTime.Now;
        [ObservableProperty] private DateTime? _datePickerBoatSillAway;

        private bool _isUpdating = false;
        [ObservableProperty] private string _title = "Добавить посещение";
        [ObservableProperty] private string _buttonCaption = "Добавить";

        public VoyageJarViewModel(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service) : base(factory, dialogManager, service)
        {
            VoyageJars = new ObservableCollection<VoyageJar>();
            Voyages = new ObservableCollection<Voyage>();
            Jars = new ObservableCollection<Jar>();
        }

        static public async Task<VoyageJarViewModel> CreateAsync(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service)
        {
            var vm = new VoyageJarViewModel(factory, dialogManager, service);
            await vm.LoadAsync();
            return vm;
        }

        public async Task LoadAsync()
        {
            VoyageJars = [.. await Service.VoyageJarRep.GetAllAsync(vj => vj.Voyage, vj => vj.Jar)];
            Voyages = [.. await Service.VoyageRep.GetAllAsync()];
            Jars = [.. await Service.JarRep.GetAllAsync()];
        }

        [RelayCommand]
        private async Task ApplyOperationAsync()
        {
            if (_isUpdating)
                await ExecuteSaveAsync(async () =>
                {
                    VoyageJar? selectedVoyageJar = SelectedVoyageJar;

                    if(selectedVoyageJar == null)
                        throw new InvalidOperationException("Вы не выбрали запись!");

                    if (SelectedJar == null)
                            throw new InvalidOperationException("Вы не выбрали банку!");

                    if (SelectedVoyage == null)
                        throw new InvalidOperationException("Вы не выбрали рейс!");
                    
                    await Service.UpdateVoyageJarAsync(
                        selectedVoyageJar,
                        SelectedVoyage.Id,
                        SelectedJar.Id,
                        DatePickerBoatArrive,
                        DatePickerBoatSillAway);

                    int index = VoyageJars.IndexOf(selectedVoyageJar);
                    VoyageJars.RemoveAt(index);
                    VoyageJars.Insert(index, selectedVoyageJar);
                });
            else
                await ExecuteSaveAsync(async () => 
                {
                    if (SelectedJar == null)
                        throw new InvalidOperationException("Вы не выбрали банку!");

                    if (SelectedVoyage == null)
                        throw new InvalidOperationException("Вы не выбрали рейс!");

                    VoyageJar newVoyageJar = new VoyageJar();

                    await Service.AddVoyageJarAsync(
                        newVoyageJar,
                        SelectedVoyage.Id,
                        SelectedJar.Id,
                        DatePickerBoatArrive,
                        DatePickerBoatSillAway);

                    newVoyageJar.Voyage = SelectedVoyage;
                    newVoyageJar.Jar = SelectedJar;

                    VoyageJars.Add(newVoyageJar);
                    await ClearFormAsync();
                });
        }

        [RelayCommand]
        private async Task RemoveAsync()
        {
            await ExecuteSaveAsync(async () =>
            {
                var selectedVoyageJar = SelectedVoyageJar;

                if (selectedVoyageJar == null)
                    throw new Exception("Вы не выбрали запись");

                if (DialogManager.ShowConfirmation("Вы точно хотите удалить запись?"))
                {
                    await Service.VoyageJarRep.DeleteAsync(selectedVoyageJar);
                    VoyageJars.Remove(selectedVoyageJar);
                    await PrepareToAdd();
                }
            });
        }

        public async Task PrepareToUpdateAsync(VoyageJar selectedVoyageJar)
        {
            _isUpdating = true;
            Title = "Изменить посещение";
            ButtonCaption = "Изменить";
            await FillFormAsync(selectedVoyageJar);
        }

        [RelayCommand]
        private async Task PrepareToAdd()
        {
            _isUpdating = false;
            Title = "Добавить посещение";
            ButtonCaption = "Добавить";
            await ClearFormAsync();
        }

        private async Task FillFormAsync(VoyageJar voyageJar)
        {
            SelectedJar = voyageJar.Jar;
            SelectedVoyage = voyageJar.Voyage;
            DatePickerBoatArrive = voyageJar.BoatArrive;
            DatePickerBoatSillAway = voyageJar.BoatSillAway;
        }

        private async Task ClearFormAsync()
        {
            DatePickerBoatArrive = DateTime.Now;
            DatePickerBoatSillAway = null;
            SelectedJar = null;
            SelectedVoyage = null;
        }
    }
}
