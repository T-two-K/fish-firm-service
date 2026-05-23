using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ProgrammingPractice_L19.Database;
using ProgrammingPractice_L19.Model;
using ProgrammingPractice_L19.Service;
using System.Collections.ObjectModel;

namespace ProgrammingPractice_L19.ViewModel
{
    public partial class FishermenViewModel : BaseViewModel
    {
        public ObservableCollection<Fisherman> Fishermen { get; set; }
        [NotifyCanExecuteChangedFor(nameof(RemoveCommand))]
        [ObservableProperty] private Fisherman? _selectedFisherman;

        [ObservableProperty] private string _title = "Добавить нового рыбака";
        [ObservableProperty] private string _buttonCaption = "Добавить";
        [ObservableProperty] private bool _isUpdating = false;

        [ObservableProperty] private string _textBoxFullName = string.Empty;
        [ObservableProperty] private string _textBoxJobTitle = string.Empty;
        [ObservableProperty] private string _textBoxAddress = string.Empty;

        public FishermenViewModel(IDbContextFactory<FishFirmDbContext> factory,
            IDialogManagerService dialogManager,
            FishFirmService service) : base(factory, dialogManager, service)
        {
            List<Fisherman> fishermen = Service.FishermanRep.GetAll();

            Fishermen = [.. fishermen];
        }

        [RelayCommand]
        private void ApplyOperation()
        {
            if (IsUpdating)
            {
                ExecuteSave(() =>
                {
                    Fisherman? selectedFisherman = SelectedFisherman;
                    if (selectedFisherman == null)
                        throw new InvalidOperationException("Рыбак не выбран.");

                    if (selectedFisherman.FullName != TextBoxFullName)
                        if (Service.FishermanRep.CheckBy(f => f.FullName == TextBoxFullName))
                            throw new InvalidOperationException("Рыбак с таким именем уже есть.");

                    int index = Fishermen.IndexOf(selectedFisherman);

                    selectedFisherman.FullName = TextBoxFullName;
                    selectedFisherman.JobTitle = TextBoxJobTitle;
                    selectedFisherman.Address = TextBoxAddress;

                    Service.FishermanRep.Update(selectedFisherman);
                    Fishermen.RemoveAt(index);
                    Fishermen.Insert(index, selectedFisherman);
                });
            }
            else
            {
                ExecuteSave(() =>
                {
                    if (Service.FishermanRep.CheckBy(f => f.FullName == TextBoxFullName))
                        throw new InvalidOperationException("Рыбак с таким именем уже есть.");

                    Fisherman fihserman = new Fisherman()
                    {
                        FullName = TextBoxFullName,
                        Address = TextBoxAddress,
                        JobTitle = TextBoxJobTitle,
                    };

                    Service.FishermanRep.Add(fihserman);
                    Fishermen.Add(fihserman);
                    ClearForm();
                });
            }
        }

        [RelayCommand(CanExecute = nameof(CheckSelected))]
        private void Remove()
        {
            if (DialogManager.ShowConfirmation("Вы точно хотите удалить рыбака?"))
                ExecuteSave(() =>
                {
                    if (SelectedFisherman == null)
                        throw new InvalidOperationException("Рыбак не выбран.");

                    Service.FishermanRep.Remove(SelectedFisherman);
                    Fishermen.Remove(SelectedFisherman);
                });
        }

        private bool CheckSelected() => SelectedFisherman != null;

        public void PrepareToUpdate()
        {
            IsUpdating = true;
            Title = "Изменить рыбака";
            ButtonCaption = "Изменить";
            FillForm();
        }

        [RelayCommand]
        private void PrepareToAdd()
        {
            IsUpdating = false;
            Title = "Добавить новго рыбака";
            ButtonCaption = "Добавить";
            ClearForm();
        }

        private void ClearForm()
        {
            TextBoxAddress = string.Empty;
            TextBoxFullName = string.Empty;
            TextBoxJobTitle = string.Empty;
        }

        private void FillForm()
        {
            ExecuteSave(() =>
            {
                if (SelectedFisherman == null)
                    throw new InvalidOperationException("Вы не выбрали рыбака.");

                TextBoxAddress = SelectedFisherman.Address;
                TextBoxFullName = SelectedFisherman.FullName;
                TextBoxJobTitle = SelectedFisherman.JobTitle;
            });
        }


        private void ExecuteSave(Action operation)
        {
            try
            {
                operation();
            }
            catch (Exception ex)
            {
                DialogManager.ShowErrorWindow(ex.Message);
            }
        }
    }
}
