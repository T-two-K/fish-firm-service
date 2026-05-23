using Microsoft.EntityFrameworkCore;
using ProgrammingPractice_L19.Database;
using ProgrammingPractice_L19.Service;
using ProgrammingPractice_L19.DTO;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ProgrammingPractice_L19.Model;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Tools.Extension;

namespace ProgrammingPractice_L19.ViewModel
{
    public partial class VoyagesByJarAndFishViewModel : BaseViewModel
    {
        public ObservableCollection<VoyageByJarAndQuality> VoyagesByJarQal { get; set; }

        public ObservableCollection<Jar> Jars { get; set; }
        [ObservableProperty] private Jar? _selectedJar;
        public ObservableCollection<string> Qualities { get; set; }
        [ObservableProperty] private string? _selectedQuality;

        public VoyagesByJarAndFishViewModel(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service) : base(factory, dialogManager, service)
        {
            VoyagesByJarQal = new ObservableCollection<VoyageByJarAndQuality>();
            Jars = new ObservableCollection<Jar>();
            Qualities = new ObservableCollection<string>();
        }

        static public async Task<VoyagesByJarAndFishViewModel> CreateAsync(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service)
        {
            var vm = new VoyagesByJarAndFishViewModel(factory, dialogManager, service);
            await vm.LoadAsync();
            return vm;
        }

        private async Task LoadAsync()
        {
            var quality = await Service.FishGroupRep.GetAllQualitiesAsync();

            foreach (var qual in quality)
                if (!string.IsNullOrWhiteSpace(qual))
                    Qualities.Add(qual);

            Jars.AddRange(await Service.JarRep.GetAllAsync(j => j.Voyages));
        }

        [RelayCommand]
        private async Task ShowVoyages()
        {
            VoyagesByJarQal.Clear();

            if (SelectedJar == null || SelectedQuality == null)
                throw new Exception("Вы не выбрали банку или сорт рыбы!");

            VoyagesByJarQal.AddRange(await Service.FishGroupRep
                    .GetVoyageBy(SelectedJar.Name, SelectedQuality));
        }
    }
}
