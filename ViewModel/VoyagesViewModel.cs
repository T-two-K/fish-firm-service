using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Tools.Extension;
using Microsoft.EntityFrameworkCore;
using ProgrammingPractice_L19.Database;
using ProgrammingPractice_L19.Model;
using ProgrammingPractice_L19.Service;
using System.Collections.ObjectModel;

namespace ProgrammingPractice_L19.ViewModel
{
    public partial class VoyagesViewModel : BaseViewModel
    {
        public ObservableCollection<Voyage> Voyages { get; set; }
        [NotifyCanExecuteChangedFor(nameof(RemoveCommand))]
        [ObservableProperty] private Voyage? _selectedVoyage;

        [ObservableProperty] private bool _isUpdating = false;
        [ObservableProperty] private string _title = "Добавить рейс";
        [ObservableProperty] private string _buttonCaption = "Добавить";

        [ObservableProperty] private string _textBoxVoyageNumber = string.Empty;
        [ObservableProperty] private DateTime _datePickerStartDate = DateTime.Now;

        public ObservableCollection<Fisherman> VoyageFishermen { get; set; }
        [ObservableProperty] private Fisherman? _selectedFisherman;
        public ObservableCollection<Fisherman> VoyageAllFishermen { get; set; }

        public ObservableCollection<Boat> FreeBoats { get; set; }
        [ObservableProperty] private Boat? _selectedBoat;

        public VoyagesViewModel(IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service) : base(factory, dialogManager, service)
        {
            Voyages = new ObservableCollection<Voyage>();
            VoyageFishermen = new ObservableCollection<Fisherman>();
            VoyageAllFishermen = new ObservableCollection<Fisherman>();
            FreeBoats = new ObservableCollection<Boat>();
        }

        static public async Task<VoyagesViewModel> CreateAsync(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service)
        {
            var vm = new VoyagesViewModel(factory, dialogManager, service);
            await vm.LoadAsync();
            return vm;
        }

        private async Task LoadAsync()
        {
            List<Voyage> voyages = await Service.VoyageRep.GetAllAsync();
            List<Fisherman> fishermen = await Service.FishermanRep.GetFreeAsync();
            FreeBoats = [.. await Service.BoatRep.GetFreeAsync()];

            Voyages = [.. voyages];
            VoyageAllFishermen = [.. fishermen];
        }

        public async Task StartUpdatingAsync()
        {
            if (SelectedVoyage == null) return;

            await ExecuteSaveAsync(async () =>
            {
                if (SelectedVoyage.EndDate != null)
                    throw new InvalidOperationException("Рейс уже закончился, его нельзя изменить!");

                IsUpdating = true;
                Title = "Изменить рейс";
                ButtonCaption = "Изменить";
                await FillFormAsync(SelectedVoyage);
            });
        }

        [RelayCommand]
        private async Task StartAddingAsync()
        {
            IsUpdating = false;
            SelectedVoyage = null;
            Title = "Добавить рейс";
            ButtonCaption = "Добавить";
            await ClearFormAsync();
        }

        [RelayCommand]
        private async Task ApplyOperation()
        {
            if (IsUpdating)
            {
                await ExecuteSaveAsync(async () =>
                {
                    var selectedVoyage = SelectedVoyage
                        ?? throw new InvalidOperationException("Рейс не был выбран.");

                    if (string.IsNullOrWhiteSpace(TextBoxVoyageNumber))
                        throw new InvalidOperationException("Строка с номером рейса пуста!");

                    if (selectedVoyage.EndDate != null)
                        throw new InvalidOperationException("Рейс завершён, изменить нельзя.");

                    if (SelectedBoat == null)
                        throw new InvalidOperationException("Вы не выбрали лодку.");

                    await Service.UpdateVoyageAsync(
                        selectedVoyage,
                        SelectedBoat.Id,
                        VoyageFishermen.ToList(),
                        DatePickerStartDate,
                        TextBoxVoyageNumber);

                    int index = Voyages.IndexOf(selectedVoyage);
                    Voyages.RemoveAt(index);
                    Voyages.Insert(index, selectedVoyage);
                });
            }
            else
            {
                await ExecuteSaveAsync(async () =>
                {
                    if (SelectedBoat == null)
                        throw new InvalidOperationException("Вы не выбрали лодку.");

                    Voyage voyage = new Voyage()
                    {
                        VoyageNumber = TextBoxVoyageNumber,
                        StartDate = DatePickerStartDate,
                    };

                    await Service.AddVoyage(voyage,
                        SelectedBoat.Id, VoyageFishermen.ToList());

                    Voyages.Add(voyage);
                });
            }
        }

        [RelayCommand(CanExecute = nameof(CheckVoyageSelected))]
        private async Task RemoveAsync()
        {
            if (DialogManager.ShowConfirmation("Вы точно хотите удалить рейс?"))
                await ExecuteSaveAsync(async () =>
                {
                    Voyage selectedVoyage = SelectedVoyage ??
                        throw new InvalidOperationException("Рейс не выбран.");

                    if (selectedVoyage.EndDate == null)
                        throw new InvalidOperationException("Перед удалением рейса, его необходимо завершить");

                    await Service.RemoveVoyageAsync(selectedVoyage);
                    Voyages.Remove(selectedVoyage);
                });
        }

        [RelayCommand]
        private async Task FinishVoyageAsync()
        {
            if (DialogManager.ShowConfirmation("ВНИМАНИЕ! После завершения рейса" +
                " дату его окончания изменить невозможно, вы точно хотите его завершить?"))
                await ExecuteSaveAsync(async () =>
                {
                    var selectedVoyage = SelectedVoyage;
                    if (selectedVoyage == null)
                        throw new InvalidOperationException("Рейс не был выбран!");

                    if (selectedVoyage.EndDate != null)
                        throw new InvalidOperationException("Рейс уже завершён!");

                    await Service.FinishVoyageAsync(selectedVoyage);

                    VoyageAllFishermen.Clear();
                    VoyageAllFishermen.AddRange(await Service.FishermanRep.GetFreeAsync());

                    await ClearFormAsync();

                    int index = Voyages.IndexOf(selectedVoyage);
                    Voyages.RemoveAt(index);
                    Voyages.Insert(index, selectedVoyage);
                });
        }

        [RelayCommand]
        private void AddFisherman(Fisherman fisherman)
        {
            VoyageFishermen.Add(fisherman);
            VoyageAllFishermen.Remove(fisherman);
        }

        [RelayCommand]
        private void RemoveFisherman(Fisherman fisherman)
        {
            VoyageAllFishermen.Add(fisherman);
            VoyageFishermen.Remove(fisherman);
        }

        private bool CheckVoyageSelected() => SelectedVoyage != null;

        private async Task FillFormAsync(Voyage voyage)
        {
            await RemoveBusyBoatsAsync();
            TextBoxVoyageNumber = voyage.VoyageNumber;
            DatePickerStartDate = voyage.StartDate;
            VoyageFishermen.Clear();
            VoyageFishermen.AddRange(voyage.Fishermen.Select(f => f.Fisherman).ToList());

            if (!FreeBoats.Contains(voyage.CurrentBoat))
                FreeBoats.Add(voyage.CurrentBoat);

            SelectedBoat = voyage.CurrentBoat;
        }

        private async Task ClearFormAsync()
        {
            await RemoveBusyBoatsAsync();
            TextBoxVoyageNumber = string.Empty;
            DatePickerStartDate = DateTime.Now;
            VoyageFishermen.Clear();
            SelectedBoat = null;
        }

        private async Task RemoveBusyBoatsAsync()
        {
            var busyBoats = FreeBoats.Where(b => b.IsBusy).ToList();
            foreach (var busyBoat in busyBoats)
                FreeBoats.Remove(busyBoat);
        }
    }
}
