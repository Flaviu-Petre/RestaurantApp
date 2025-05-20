using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RestaurantApp.UI.ViewModels.Admin
{
    public class StockUpdateViewModel : ViewModelBase
    {
        private readonly LowStockItemViewModel _item;
        private readonly IDishService _dishService;
        private readonly IDialogService _dialogService;

        public StockUpdateViewModel(
            LowStockItemViewModel item,
            IDishService dishService,
            IDialogService dialogService)
        {
            _item = item;
            _dishService = dishService;
            _dialogService = dialogService;

            // Setup properties
            ItemName = item.Name;
            CurrentStock = item.TotalQuantity;
            NewStock = item.TotalQuantity;

            // Initialize commands
            UpdateStockCommand = new AsyncRelayCommand(UpdateStockAsync, CanUpdateStock);
            CancelCommand = new RelayCommand(Cancel);
        }

        #region Properties

        public string ItemName { get; }
        public decimal CurrentStock { get; }

        private decimal _newStock;
        public decimal NewStock
        {
            get => _newStock;
            set
            {
                if (SetProperty(ref _newStock, value))
                {
                    (UpdateStockCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _dialogResult = false;
        public bool DialogResult
        {
            get => _dialogResult;
            set => SetProperty(ref _dialogResult, value);
        }

        #endregion

        #region Commands

        public ICommand UpdateStockCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Methods

        private async Task UpdateStockAsync()
        {
            try
            {
                IsBusy = true;

                // Update the dish stock
                await _dishService.UpdateDishQuantityAsync(_item.Id, NewStock);

                // Update the view model
                _item.TotalQuantity = NewStock;

                // Set success result
                DialogResult = true;

                // Close the dialog
                CloseDialog();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating stock: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanUpdateStock()
        {
            return NewStock >= 0 && NewStock != CurrentStock;
        }

        private void Cancel()
        {
            DialogResult = false;
            CloseDialog();
        }

        private void CloseDialog()
        {
            // Get the window that this view model is attached to
            if (Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this) is Window window)
            {
                window.DialogResult = DialogResult;
                window.Close();
            }
        }

        #endregion
    }
}