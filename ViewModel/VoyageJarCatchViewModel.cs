using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Tools.Extension;
using Microsoft.EntityFrameworkCore;
using ProgrammingPractice_L19.Database;
using ProgrammingPractice_L19.DTO;
using ProgrammingPractice_L19.Model;
using ProgrammingPractice_L19.Service;
using ProgrammingPractice_L19.ViewModel;
using System.Collections.ObjectModel;

namespace ProgrammingPractice_L19.ViewModel;

public partial class VoyageJarCatchViewModel : BaseViewModel
{
    public List<Voyage> FinishedVoyages { get; set; }
    [ObservableProperty] private Voyage? _selectedVoyage;

    public ObservableCollection<FishGroup> CatchedFish { get; set; }
    [ObservableProperty] private FishGroup? _selectedFish;

    public ObservableCollection<Jar> Jars { get; set; }
    [ObservableProperty] private Jar? _selectedJar;

    [ObservableProperty] private string _textBoxQuality = string.Empty;
    [ObservableProperty] private string _textBoxWeight = string.Empty;

    public ObservableCollection<VoyageJarCatchReportDto> DisplayVoyageJar { get; set; }
    [ObservableProperty] private VoyageJarCatchReportDto? _selectedCatchReport;

    [ObservableProperty] private string _title = "Добавить улов";
    [ObservableProperty] private string _buttonCaption = "Добавить";

    private bool _isUpdating = false;

    private VoyageJarCatchViewModel(
        IDbContextFactory<FishFirmDbContext> factory,
        IDialogManagerService dialogManager,
        FishFirmService service) : base(factory, dialogManager, service)
    {
        Jars = new ObservableCollection<Jar>();
        CatchedFish = new ObservableCollection<FishGroup>();
        FinishedVoyages = new List<Voyage>();
        DisplayVoyageJar = new ObservableCollection<VoyageJarCatchReportDto>();
    }

    static public async Task<VoyageJarCatchViewModel> CreateAsync(
        IDbContextFactory<FishFirmDbContext> factory,
        IDialogManagerService dialogManager,
        FishFirmService service)
    {
        var vm = new VoyageJarCatchViewModel(factory, dialogManager, service);
        await vm.LoadAsync();
        return vm;
    }

    private async Task LoadAsync()
    {
        FinishedVoyages = await Service.VoyageRep.GetFinishedAsync();
        DisplayVoyageJar.AddRange(await Service.VoyageJarRep.GetDataDtoAsync());
    }

    [RelayCommand]
    private async Task ApplyOperationAsync()
    {
        await ExecuteSaveAsync(async () =>
        {
            if (!double.TryParse(TextBoxWeight, out double weight))
                throw new InvalidOperationException("Некорректное значение веса!");

            if (string.IsNullOrWhiteSpace(TextBoxQuality))
                throw new InvalidOperationException("Вы не указали качесвто!");

            if (TextBoxQuality.Length > 15)
                throw new InvalidOperationException("Качество имеет слишком много символов! (Max 15)");

            if (SelectedVoyage == null)
                throw new InvalidOperationException("Вы не выбрали рейс!");

            if (SelectedJar == null)
                throw new InvalidOperationException("Вы не выбрали банку!");

            if (SelectedFish == null)
                throw new InvalidOperationException("Вы не выбрали рыбу!");

            await Service.AddFishInfoBy(
                SelectedVoyage, SelectedJar,
                SelectedFish, TextBoxQuality, weight);

            await RefreshDisplayAsync();
            await ClearFormAsync();
        });
    }

    private async Task RefreshDisplayAsync()
    {
        DisplayVoyageJar.Clear();
        DisplayVoyageJar.AddRange(await Service.VoyageJarRep.GetDataDtoAsync());
    }

    public async Task StartUpdatingAsync()
    {
        if (SelectedCatchReport == null) return;

        _isUpdating = true;
        Title = "Изменить улов";
        ButtonCaption = "Изменить";
        await FillFormAsync(SelectedCatchReport);
    }

    [RelayCommand]
    private async Task StartAddingAsync()
    {
        _isUpdating = false;
        Title = "Добавить улов";
        ButtonCaption = "Добавить";
        await ClearFormAsync();
    }

    private async Task FillFormAsync(VoyageJarCatchReportDto voyageReport)
    {
        SelectedVoyage = FinishedVoyages
            .FirstOrDefault(v => v.VoyageNumber == voyageReport.VoyageNumber);

        if (SelectedVoyage == null)
            throw new InvalidOperationException("Рейс не найден.");

        SelectedJar = Jars.FirstOrDefault(j => j.Name == voyageReport.JarName);

        if (SelectedJar == null)
            throw new InvalidOperationException("Банка не найдена.");

        await LoadFishForSelectedJarAsync();

        SelectedFish = CatchedFish.FirstOrDefault(f =>
            f.Name == voyageReport.FishName &&
            f.Quality == voyageReport.FishQuality);

        TextBoxQuality = voyageReport.FishQuality ?? string.Empty;
        TextBoxWeight = voyageReport.FishWeight?.ToString() ?? string.Empty;
    }

    partial void OnSelectedVoyageChanged(Voyage? value)
    {
        Jars.Clear();
        CatchedFish.Clear();
        SelectedJar = null;

        if (value == null) return;

        Jars.AddRange(value.Jars
            .Select(j => j.Jar)
            .OrderBy(j => j.Name));

        SelectedJar = Jars.FirstOrDefault();
    }

    partial void OnSelectedJarChanged(Jar? value)
    {
        _ = LoadFishForSelectedJarAsync();
    }

    private async Task LoadFishForSelectedJarAsync()
    {
        CatchedFish.Clear();
        SelectedFish = null;

        if (SelectedJar == null || SelectedVoyage == null) return;

        var fish = await Service.FishGroupRep
            .GetByKeys(SelectedJar.Id, SelectedVoyage.Id);

        CatchedFish.AddRange(fish);
        SelectedFish = CatchedFish.FirstOrDefault();
    }

    private async Task ClearFormAsync()
    {
        TextBoxWeight = string.Empty;
        TextBoxQuality = string.Empty;
        SelectedVoyage = null;
        SelectedFish = null;
        SelectedJar = null;
        _isUpdating = false;
    }
}