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
    public partial class JarCatchViewModel : BaseViewModel
    {
        public ObservableCollection<JarsAverageCatchDto> AverageCatch { get; set;}

        [ObservableProperty] private DateTime? _datePickerStartDate;

        [ObservableProperty] private DateTime? _datePickerEndDate;

        public JarCatchViewModel(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service) : base(factory, dialogManager, service)
        {
            AverageCatch = new ObservableCollection<JarsAverageCatchDto>();
        }

        static public async Task<JarCatchViewModel> CreateAsync(
            IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service)
        {
            var vm = new JarCatchViewModel(factory, dialogManager, service);
            await vm.LoadAsync();
            return vm;
        }

        private async Task LoadAsync() =>
            AverageCatch.AddRange(await Service.VoyageJarRep
                            .GetAverageCatch(DateTime.MinValue, DateTime.MaxValue));

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

                AverageCatch.Clear();
                AverageCatch.AddRange(await Service.VoyageJarRep
                                .GetAverageCatch(DatePickerStartDate, DatePickerEndDate));
            });
        }
    }
}
