using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Tools.Extension;
using Microsoft.EntityFrameworkCore;
using ProgrammingPractice_L19.Database;
using ProgrammingPractice_L19.DTO;
using ProgrammingPractice_L19.Model;
using ProgrammingPractice_L19.Service;
using System.Collections.ObjectModel;

namespace ProgrammingPractice_L19.ViewModel
{
    public partial class MoreThanAverageBoatCatchViewModel : BaseViewModel
    {
        public ObservableCollection<BoatMoreThenAverageCatchDto> AverageBoatCatch { get; set; }
        public ObservableCollection<Jar> Jars { get; private set; }
        [ObservableProperty] private Jar? _selectedJar;

        [ObservableProperty] private double? _jarAverageCatch;

        public MoreThanAverageBoatCatchViewModel(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service) : base(factory, dialogManager, service)
        {
            AverageBoatCatch = new ObservableCollection<BoatMoreThenAverageCatchDto>();
            Jars = new ObservableCollection<Jar>();
        }

        static public async Task<MoreThanAverageBoatCatchViewModel> CreateAsync(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service)
        {
            var vm = new MoreThanAverageBoatCatchViewModel(factory, dialogManager, service);
            await vm.LoadAsync();
            return vm;
        }

        private async Task LoadAsync() =>
            Jars.AddRange(await Service.JarRep.GetAllAsync());

        async partial void OnSelectedJarChanged(Jar? value)
        {
            if (value == null) return;

            JarAverageCatch = await Service.JarRep.GetAverageCatch(value);
            AverageBoatCatch.Clear();

            AverageBoatCatch.AddRange(await Service.VoyageJarRep.GetAverageCatchBy(value.Name));
        }
    }
}
