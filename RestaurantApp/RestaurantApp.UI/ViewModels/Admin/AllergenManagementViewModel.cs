using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantApp.UI.ViewModels.Admin
{
    public class AllergenManagementViewModel : ViewModelBase
    {
        private readonly IAllergenService _allergenService;
        private readonly IDialogService _dialogService;
        private readonly IMessageBus _messageBus;

        public AllergenManagementViewModel(
            IAllergenService allergenService,
            IDialogService dialogService,
            IMessageBus messageBus)
        {
            _allergenService = allergenService;
            _dialogService = dialogService;
            _messageBus = messageBus;

            // Initialize commands
            AddNewAllergenCommand = new RelayCommand(AddNewAllergen);
            SaveAllergenCommand = new AsyncRelayCommand(SaveAllergenAsync, CanSaveAllergen);
            DeleteAllergenCommand = new AsyncRelayCommand(DeleteAllergenAsync, () => SelectedAllergen != null);

            // Load data
            LoadAllergensAsync().ConfigureAwait(false);
        }

        #region Properties

        private ObservableCollection<Allergen> _allergens;
        public ObservableCollection<Allergen> Allergens
        {
            get => _allergens;
            set => SetProperty(ref _allergens, value);
        }

        private Allergen _selectedAllergen;
        public Allergen SelectedAllergen
        {
            get => _selectedAllergen;
            set
            {
                if (SetProperty(ref _selectedAllergen, value))
                {
                    // Update properties based on selected allergen
                    AllergenName = _selectedAllergen?.Name ?? string.Empty;
                    AllergenDescription = _selectedAllergen?.Description ?? string.Empty;
                    OnPropertyChanged(nameof(HasSelectedAllergen));
                }
            }
        }

        private string _allergenName;
        public string AllergenName
        {
            get => _allergenName;
            set => SetProperty(ref _allergenName, value);
        }

        private string _allergenDescription;
        public string AllergenDescription
        {
            get => _allergenDescription;
            set => SetProperty(ref _allergenDescription, value);
        }

        public bool HasSelectedAllergen => SelectedAllergen != null;

        #endregion

        #region Commands

        public ICommand AddNewAllergenCommand { get; }
        public ICommand SaveAllergenCommand { get; }
        public ICommand DeleteAllergenCommand { get; }

        #endregion

        #region Methods

        private async Task LoadAllergensAsync()
        {
            try
            {
                IsBusy = true;
                var allergens = await _allergenService.GetAllAllergensAsync();
                Allergens = new ObservableCollection<Allergen>(allergens);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Error loading allergens: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AddNewAllergen()
        {
            // Create a new allergen and select it
            SelectedAllergen = new Allergen();
            AllergenName = string.Empty;
            AllergenDescription = string.Empty;
        }

        private async Task SaveAllergenAsync()
        {
            try
            {
                IsBusy = true;

                // Update allergen properties
                bool isNewAllergen = SelectedAllergen.Id == 0;
                SelectedAllergen.Name = AllergenName;
                SelectedAllergen.Description = AllergenDescription;

                if (isNewAllergen)
                {
                    // Create new allergen
                    var createdAllergen = await _allergenService.CreateAllergenAsync(SelectedAllergen);
                    Allergens.Add(createdAllergen);
                    SelectedAllergen = createdAllergen;
                }
                else
                {
                    // Update existing allergen
                    await _allergenService.UpdateAllergenAsync(SelectedAllergen);

                    // Refresh the collection to update the UI
                    int index = Allergens.IndexOf(Allergens.FirstOrDefault(a => a.Id == SelectedAllergen.Id));
                    if (index >= 0)
                    {
                        Allergens[index] = SelectedAllergen;
                    }
                }

                _dialogService.ShowMessage("Allergen saved successfully.", "Success",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                // Notify that data has changed
                _messageBus.Publish(new RefreshDataMessage());
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Error saving allergen: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteAllergenAsync()
        {
            if (SelectedAllergen == null)
                return;

            var result = _dialogService.ShowMessage("Are you sure you want to delete this allergen?",
                "Confirm Delete", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    await _allergenService.DeleteAllergenAsync(SelectedAllergen.Id);

                    // Remove from collection
                    Allergens.Remove(SelectedAllergen);
                    SelectedAllergen = null;

                    // Notify that data has changed
                    _messageBus.Publish(new RefreshDataMessage());

                    _dialogService.ShowMessage("Allergen deleted successfully.", "Success",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessage($"Error deleting allergen: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private bool CanSaveAllergen()
        {
            return !string.IsNullOrWhiteSpace(AllergenName);
        }

        #endregion
    }
}