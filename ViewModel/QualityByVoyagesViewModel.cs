using HandyControl.Tools.Extension;
using Microsoft.EntityFrameworkCore;
using ProgrammingPractice_L19.Database;
using ProgrammingPractice_L19.DTO;
using ProgrammingPractice_L19.Service;
using System.Collections.ObjectModel;

namespace ProgrammingPractice_L19.ViewModel
{
    public partial class QualityByVoyagesViewModel : BaseViewModel
    {
        public ObservableCollection<QualityCatchByVoyagesDto> QualityCatch { get; private set; }
        public List<string?> Qualities { get; private set; }

        private QualityByVoyagesViewModel(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service) : base(factory, dialogManager, service)
        {
            QualityCatch = new ObservableCollection<QualityCatchByVoyagesDto>();
            Qualities = new List<string?>();
        }

        static public async Task<QualityByVoyagesViewModel> CreateAsync(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service)
        {
            var vm = new QualityByVoyagesViewModel(factory, dialogManager, service);
            await vm.LoadAsync();
            return vm;
        }

        private async Task LoadAsync()
        {
            Qualities = await Service.FishGroupRep.GetAllQualitiesAsync();

            foreach (var qual in Qualities)
                if (!string.IsNullOrWhiteSpace(qual))
                    QualityCatch.AddRange(await Service.VoyageJarRep.GetQualityCatch(qual));
        }
    }
}
