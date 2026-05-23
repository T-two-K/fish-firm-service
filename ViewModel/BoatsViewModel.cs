using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ProgrammingPractice_L19.Database;
using ProgrammingPractice_L19.Model;
using ProgrammingPractice_L19.Service;
using System.Collections.ObjectModel;

namespace ProgrammingPractice_L19.ViewModel
{
    public partial class BoatsViewModel : BaseViewModel
    {
        public ObservableCollection<Boat> Boats { get; set; }
        [ObservableProperty] private Boat? _selectedBoat;

        [ObservableProperty] private bool _isUpdating = false;

        [ObservableProperty] private string _title = "Добавить новый катер";
        [ObservableProperty] private string _buttonCaption = "Добавить";

        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _type = string.Empty;
        [ObservableProperty] private string _displacement = string.Empty;
        [ObservableProperty] private DateTime? _cunstructionDate;

        private BoatsViewModel(IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service) : base(factory, dialogManager, service)
        {
            Boats = new ObservableCollection<Boat>();
        }

        static public async Task<BoatsViewModel> CreateAsync(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service)
        {
            var vm = new BoatsViewModel(factory, dialogManager, service);
            await vm.LoadAsync();
            return vm;
        }

        public async Task StartUpdatingAsync()
        {
            if (SelectedBoat == null) return;

            IsUpdating = true;
            Title = "Изменить катер";
            ButtonCaption = "Изменить";
            await FillFormAsync(SelectedBoat);
        }

        [RelayCommand]
        private async Task StartAddingAsync()
        {
            await ExecuteSaveAsync(async () =>
            {
                IsUpdating = false;
                SelectedBoat = null;
                Title = "Добавить новый катер";
                ButtonCaption = "Добавить";
                await ClearFormAsync();
            });
        }

        [RelayCommand]
        private async Task ApplyOperation()
        {
            if (IsUpdating)
            {
                var selectedBoat = SelectedBoat;

                if (selectedBoat == null) return;

                await ExecuteSaveAsync(async () =>
                {
                    if (!double.TryParse(Displacement, out double displacement))
                        throw new InvalidOperationException("В водоизмещение было записано некорректное значение!");

                    if (string.IsNullOrWhiteSpace(Type))
                        throw new InvalidOperationException("Вы не указали тип лодки!");

                    if (string.IsNullOrWhiteSpace(Name))
                        throw new InvalidOperationException("Вы не указали имя лодки!");

                    if (await Service.BoatRep.CheckExist(b => b.Name == Name)
                        && await Service.BoatRep.CheckExist(b => b.Type == Type))
                        throw new InvalidOperationException("Лодка с таким названием и типом уже существует!");

                    selectedBoat.Type = Type;
                    selectedBoat.Name = Name;
                    selectedBoat.Displacement = displacement;
                    selectedBoat.ConstructionDate = CunstructionDate ?? DateTime.Now;

                    await Service.BoatRep.UpdateAsync(selectedBoat);
                    int jarId = Boats.IndexOf(selectedBoat);
                    Boats.RemoveAt(jarId);
                    Boats.Insert(jarId, selectedBoat);
                });
            }
            else
            {
                await ExecuteSaveAsync(async () =>
                {
                    if (!double.TryParse(Displacement, out double displacement))
                        throw new InvalidOperationException("В водоизмещение было записано некорректное значение.");

                    Boat newBoat = new Boat()
                    {
                        Name = Name,
                        Type = Type,
                        Displacement = displacement,
                        ConstructionDate = CunstructionDate ?? DateTime.Now,
                    };

                    await Service.BoatRep.AddAsync(newBoat);
                    Boats.Add(newBoat);

                    await ClearFormAsync();
                });
            }
        }

        [RelayCommand]
        private async Task Remove()
        {
            var selectedBoat = SelectedBoat;
            if (selectedBoat == null) return;

            if (DialogManager.ShowConfirmation("Вы точно хотите удалить запись?"))
            {
                await ExecuteSaveAsync(async () =>
                {
                    await Service.BoatRep.DeleteAsync(selectedBoat);
                    Boats.Remove(selectedBoat);
                });
            }
        }

        private async Task LoadAsync()
        {
            List<Boat> boats = await Service.BoatRep.GetAllAsync();
            Boats = [.. boats];
        }

        public async Task FillFormAsync(Boat boat)
        {
            Name = boat.Name;
            Type = boat.Type;
            Displacement = boat.Displacement.ToString();
            CunstructionDate = boat.ConstructionDate;
        }

        private async Task ClearFormAsync()
        {
            Name = string.Empty;
            Type = string.Empty;
            Displacement = string.Empty;
            CunstructionDate = null;
        }
    }
}
