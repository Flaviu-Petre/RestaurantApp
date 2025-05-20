using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RestaurantApp.UI.ViewModels.Admin
{
    public class LowStockViewModel : ViewModelBase
    {
        private readonly IDishService _dishService;
        private readonly IConfigurationService _configService;
        private readonly IDialogService _dialogService;

        public LowStockViewModel(
            IDishService dishService,
            IConfigurationService configService,
            IDialogService dialogService)
        {
            _dishService = dishService;
            _configService = configService;
            _dialogService = dialogService;

            // Initialize commands
            UpdateThresholdCommand = new AsyncRelayCommand(UpdateThresholdAsync);
            UpdateStockCommand = new RelayCommand<LowStockItemViewModel>(ShowUpdateStockDialog);
            RefreshCommand = new AsyncRelayCommand(LoadLowStockItemsAsync);

            // Load initial threshold from configuration
            LoadThresholdAsync().ConfigureAwait(false);

            // Load low stock items
            LoadLowStockItemsAsync().ConfigureAwait(false);
        }

        #region Properties

        private decimal _lowStockThreshold;
        public decimal LowStockThreshold
        {
            get => _lowStockThreshold;
            set => SetProperty(ref _lowStockThreshold, value);
        }

        private ObservableCollection<LowStockItemViewModel> _lowStockItems;
        public ObservableCollection<LowStockItemViewModel> LowStockItems
        {
            get => _lowStockItems;
            set => SetProperty(ref _lowStockItems, value);
        }

        public bool HasLowStockItems => LowStockItems != null && LowStockItems.Any();

        #endregion

        #region Commands

        public ICommand UpdateThresholdCommand { get; }
        public ICommand UpdateStockCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Methods

        private async Task LoadThresholdAsync()
        {
            try
            {
                LowStockThreshold = await _configService.GetLowStockThresholdAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading threshold: {ex.Message}";
            }
        }

        private async Task UpdateThresholdAsync()
        {
            try
            {
                // Validate threshold
                if (LowStockThreshold <= 0)
                {
                    _dialogService.ShowMessage("Threshold must be greater than zero.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Update app settings
                var appSettings = await _configService.GetAppSettingsAsync();
                appSettings.LowStockThreshold = LowStockThreshold;
                await _configService.UpdateAppSettingsAsync(appSettings);

                // Refresh low stock items
                await LoadLowStockItemsAsync();

                _dialogService.ShowMessage("Threshold updated successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating threshold: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadLowStockItemsAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                // Get low stock dishes from service
                var lowStockDishes = await _dishService.GetLowStockDishesAsync(LowStockThreshold);

                // Convert to view models
                var lowStockItemViewModels = lowStockDishes.Select(d => new LowStockItemViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    CategoryId = d.CategoryId,
                    CategoryName = d.Category?.Name ?? "Unknown Category",
                    PortionQuantity = d.PortionQuantity,
                    TotalQuantity = d.TotalQuantity,
                    StockStatus = GetStockStatus(d.TotalQuantity)
                }).ToList();

                LowStockItems = new ObservableCollection<LowStockItemViewModel>(lowStockItemViewModels);
                OnPropertyChanged(nameof(HasLowStockItems));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading low stock items: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string GetStockStatus(decimal quantity)
        {
            // Define thresholds for different stock levels
            decimal criticalThreshold = LowStockThreshold * 0.25m;
            decimal lowThreshold = LowStockThreshold * 0.75m;

            if (quantity <= criticalThreshold)
                return "Critical";
            else if (quantity <= lowThreshold)
                return "Low";
            else
                return "OK";
        }

        private void ShowUpdateStockDialog(LowStockItemViewModel item)
        {
            if (item == null)
                return;

            try
            {
                // Create the dialog view model
                var updateStockViewModel = new StockUpdateViewModel(
                    item,
                    _dishService,
                    _dialogService);

                // Create the dialog window
                var dialog = new RestaurantApp.UI.Views.Dialogs.StockUpdateDialog
                {
                    DataContext = updateStockViewModel,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                // Show the dialog and check the result
                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    // Refresh the low stock items
                    LoadLowStockItemsAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Error showing stock update dialog: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }

    public class LowStockItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal PortionQuantity { get; set; }
        public decimal TotalQuantity { get; set; }
        public string StockStatus { get; set; }
    }
}