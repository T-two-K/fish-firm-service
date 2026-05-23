using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Tools.Extension;
using Microsoft.EntityFrameworkCore;
using ProgrammingPractice_L19.Database;
using ProgrammingPractice_L19.DTO;
using ProgrammingPractice_L19.Service;
using System.Collections.ObjectModel;


namespace ProgrammingPractice_L19.ViewModel
{
    public partial class HigherFishCatchViewModel : BaseViewModel
    {
        public ObservableCollection<FishCatchResultDto> FishCatch { get; set; }

        [NotifyCanExecuteChangedFor(nameof(ShowCatchByCommand))]
        [ObservableProperty] private DateTime? _datePickerStartDate;

        [NotifyCanExecuteChangedFor(nameof(ShowCatchByCommand))]
        [ObservableProperty] private DateTime? _datePickerEndDate;

        private HigherFishCatchViewModel(IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service) : base(factory, dialogManager, service)
        {
            FishCatch = new ObservableCollection<FishCatchResultDto>();
        }

        static public async Task<HigherFishCatchViewModel> CreateAsync(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service)
        {
            HigherFishCatchViewModel vm = new HigherFishCatchViewModel(factory, dialogManager, service);
            await vm.LoadAsync();
            return vm;
        }

        private async Task LoadAsync() => 
            FishCatch.AddRange(await Service.FishGroupRep
                     .GetMaxFishByBoat(DateTime.MinValue, DateTime.MaxValue));

        [RelayCommand]
        private async Task ShowCatchBy()
        {
            await ExecuteSaveAsync(async () =>
            {
                if (DatePickerStartDate == null)
                    throw new Exception("Вы не указали начальную дату!");

                if (DatePickerEndDate == null)
                    throw new Exception("Вы не указали конечную дату!");

                if (DatePickerStartDate > DatePickerEndDate)
                    throw new Exception("Начальная дата не может быть больше конечной!");

                FishCatch.Clear();
                FishCatch.AddRange(await Service.FishGroupRep
                                .GetMaxFishByBoat(DatePickerStartDate, DatePickerEndDate));
            });
        }
    }
}
