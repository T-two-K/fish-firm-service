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
    public partial class FishGroupViewModel : BaseViewModel
    {
        public ObservableCollection<FishGroup> FishGroups { get; set; }
        [NotifyCanExecuteChangedFor(nameof(RemoveCommand))]
        [ObservableProperty] private FishGroup? _selectedFishGroup;

        public ObservableCollection<Voyage> Voyages { get; set; }
        [ObservableProperty] private Voyage? _selectedVoyage;

        public ObservableCollection<VoyageJar> VoyageJars { get; set; }
        [ObservableProperty] private VoyageJar? _selectedVoyageJar;

        [ObservableProperty] private string _textBoxFishName = string.Empty;

        private bool _isUpdating = false;
        [ObservableProperty] private string _title = "Добавить рыбу";
        [ObservableProperty] private string _buttonCaption = "Добавить";

        public FishGroupViewModel(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service) : base(factory, dialogManager, service)
        {
            VoyageJars = new ObservableCollection<VoyageJar>();
            Voyages = new ObservableCollection<Voyage>();
            FishGroups = new ObservableCollection<FishGroup>();
        }

        static public async Task<FishGroupViewModel> CreateAsync(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service)
        {
            var vm = new FishGroupViewModel(factory, dialogManager, service);
            await vm.LoadAsync();
            return vm;
        }

        private async Task LoadAsync()
        {
            Voyages.AddRange(await Service.VoyageRep.GetVoyageWithJarsAsync());
            FishGroups.AddRange(await Service.FishGroupRep.GetAllFishAsync());
        }

        [RelayCommand]
        private async Task ApplyOperationAsync()
        {
            if (_isUpdating)
                await ExecuteSaveAsync(UpdateFishGroupAsync);
            else
                await ExecuteSaveAsync(AddFishGroupAsync);
        }

        private async Task AddFishGroupAsync()
        {
            if (string.IsNullOrWhiteSpace(TextBoxFishName))
                throw new InvalidOperationException("Дайте рыбе название!");

            if (SelectedVoyage == null)
                throw new InvalidOperationException("Вы не выбрали рейс!");

            if (SelectedVoyageJar == null)
                throw new InvalidOperationException("Вы не выбрали посещение!");

            var newFishGroup = new FishGroup()
            {
                Name = TextBoxFishName,
                JarId = SelectedVoyageJar.JarId,
                VoyageId = SelectedVoyageJar.VoyageId,
                PeriodId = SelectedVoyageJar.PeriodId,
            };

            await using var db = await Factory.CreateDbContextAsync();
            await db.Set<FishGroup>().AddAsync(newFishGroup);
            await db.SaveChangesAsync();

            // Синхронизируем навигацию для отображения в UI
            newFishGroup.VoyageJar = SelectedVoyageJar;

            FishGroups.Add(newFishGroup);
            await ClearFormAsync();
        }

        private async Task UpdateFishGroupAsync()
        {
            if (SelectedFishGroup == null)
                throw new InvalidOperationException("Вы не выбрали группу рыб!");

            if (string.IsNullOrWhiteSpace(TextBoxFishName))
                throw new InvalidOperationException("Дайте рыбе название!");

            if (SelectedVoyage == null)
                throw new InvalidOperationException("Вы не выбрали рейс!");

            if (SelectedVoyageJar == null)
                throw new InvalidOperationException("Вы не выбрали посещение!");

            await using var db = await Factory.CreateDbContextAsync();
            await using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                bool pkChanged = SelectedFishGroup.JarId != SelectedVoyageJar.JarId
                              || SelectedFishGroup.VoyageId != SelectedVoyageJar.VoyageId
                              || SelectedFishGroup.PeriodId != SelectedVoyageJar.PeriodId;

                if (pkChanged)
                {
                    // FK изменился — удаляем и вставляем заново
                    db.Attach(SelectedFishGroup);
                    db.FishGroups.Remove(SelectedFishGroup);
                    await db.SaveChangesAsync();

                    var updatedFishGroup = new FishGroup()
                    {
                        Name = TextBoxFishName,
                        JarId = SelectedVoyageJar.JarId,
                        VoyageId = SelectedVoyageJar.VoyageId,
                        PeriodId = SelectedVoyageJar.PeriodId,
                        Quality = SelectedFishGroup.Quality,
                        Weight = SelectedFishGroup.Weight
                    };

                    await db.Set<FishGroup>().AddAsync(updatedFishGroup);
                    await db.SaveChangesAsync();

                    // Синхронизируем объект в памяти — Id мог измениться
                    int index = FishGroups.IndexOf(SelectedFishGroup);
                    updatedFishGroup.VoyageJar = SelectedVoyageJar;
                    FishGroups[index] = updatedFishGroup;
                }
                else if (SelectedFishGroup.Name != TextBoxFishName)
                {
                    await db.FishGroups
                        .Where(f => f.Id == SelectedFishGroup.Id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(f => f.Name, TextBoxFishName));

                    SelectedFishGroup.Name = TextBoxFishName;
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            await ClearFormAsync();
        }

        [RelayCommand(CanExecute = nameof(CheckFishGroupSelected))]
        private async Task RemoveAsync()
        {
            if (!DialogManager.ShowConfirmation("Вы точно хотите удалить группу рыб?"))
                return;

            await ExecuteSaveAsync(async () =>
            {
                if (SelectedFishGroup == null)
                    throw new InvalidOperationException("Вы не выбрали группу рыб!");

                await using var db = await Factory.CreateDbContextAsync();

                db.Attach(SelectedFishGroup);
                db.FishGroups.Remove(SelectedFishGroup);
                await db.SaveChangesAsync();

                FishGroups.Remove(SelectedFishGroup);
                await ClearFormAsync();
            });
        }

        partial void OnSelectedVoyageChanged(Voyage? value)
        {
            if (value == null)
            {
                VoyageJars.Clear();
                return;
            }

            VoyageJars.Clear();
            VoyageJars.AddRange(value.Jars);
        }

        [RelayCommand]
        private async Task PrepareToAddAsync()
        {
            _isUpdating = false;
            Title = "Добавить рыбу";
            ButtonCaption = "Добавить";
            await ClearFormAsync();
        }

        public async Task PrepareToUpdateAsync()
        {
            if (SelectedFishGroup == null) return;

            _isUpdating = true;
            Title = "Изменить рыбу";
            ButtonCaption = "Изменить";
            await FillFormAsync();
        }

        private bool CheckFishGroupSelected() => SelectedFishGroup != null;

        private async Task ClearFormAsync()
        {
            SelectedFishGroup = null;
            TextBoxFishName = string.Empty;
            SelectedVoyageJar = null;
            SelectedVoyage = null;
        }

        private async Task FillFormAsync()
        {
            if (SelectedFishGroup == null) return;

            TextBoxFishName = SelectedFishGroup.Name;
            SelectedVoyage = Voyages.FirstOrDefault(v => v.Id == SelectedFishGroup.VoyageId);
            SelectedVoyageJar = VoyageJars.FirstOrDefault(vj =>
                vj.JarId == SelectedFishGroup.JarId &&
                vj.VoyageId == SelectedFishGroup.VoyageId &&
                vj.PeriodId == SelectedFishGroup.PeriodId);
        }
    }
}
