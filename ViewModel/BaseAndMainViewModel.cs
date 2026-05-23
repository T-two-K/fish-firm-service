using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ProgrammingPractice_L19.Database;
using ProgrammingPractice_L19.Service;

namespace ProgrammingPractice_L19.ViewModel;

public partial class BaseViewModel : ObservableObject
{
    protected IDbContextFactory<FishFirmDbContext> Factory { get; set; }
    protected IDialogManagerService DialogManager { get; set; }
    protected FishFirmService Service { get; set; }

    public BaseViewModel(IDbContextFactory<FishFirmDbContext> factory,
        IDialogManagerService dialogManager, FishFirmService service)
    {
        Factory = factory;
        Service = service;
        DialogManager = dialogManager;
    }

    public async Task ExecuteSaveAsync(Func<Task> task)
    {
        try
        {
            await task();
        }
        catch (InvalidOperationException ex)
        {
            DialogManager.ShowErrorWindow(ex.Message);
        }
    }
}

public partial class MainViewModel : BaseViewModel
{
    [ObservableProperty] private BaseViewModel? _currentVM;
    [ObservableProperty] private string _section = "base";
    public bool IsAdmin { get; set; }

    public MainViewModel(
        IDbContextFactory<FishFirmDbContext> factory,
        IDialogManagerService dialogManager,
        FishFirmService service, bool isAdmin) : base(factory, dialogManager, service) 
    {
        IsAdmin = isAdmin;
    }

    [RelayCommand]
    private async Task NavigateTo(string section)
    {
        Section = section;
        CurrentVM = section switch
        {
            //для проверки асинхронной и синхронной работы
            "fishermen" => new FishermenViewModel(Factory, DialogManager, Service),
            "boats" => await BoatsViewModel.CreateAsync(Factory, DialogManager, Service),
            "jars" => await JarsViewModel.CreateAsync(Factory, DialogManager, Service),
            "voyagejars" => await VoyageJarViewModel.CreateAsync(Factory, DialogManager, Service),
            "voyages" => await VoyagesViewModel.CreateAsync(Factory, DialogManager, Service),
            "fishgroups" => await FishGroupViewModel.CreateAsync(Factory, DialogManager, Service),
            "voyagejarscatch" => await VoyageJarCatchViewModel.CreateAsync(Factory, DialogManager, Service),
            "highercatch" => await HigherFishCatchViewModel.CreateAsync(Factory, DialogManager, Service),
            "averagecatchbyjar" => await JarCatchViewModel.CreateAsync(Factory, DialogManager, Service),
            "morethanaverage" => await MoreThanAverageBoatCatchViewModel.CreateAsync(Factory, DialogManager, Service),
            "voyagesbyjarandfish" => await VoyagesByJarAndFishViewModel.CreateAsync(Factory, DialogManager, Service),
            "qualitylist" => await QualityByVoyagesViewModel.CreateAsync(Factory, DialogManager, Service),
            _ => null
        };
    }
}

