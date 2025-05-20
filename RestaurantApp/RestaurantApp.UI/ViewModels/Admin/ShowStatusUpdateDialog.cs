using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RestaurantApp.UI.ViewModels.Admin
{
    public class StatusUpdateViewModel : ViewModelBase
    {
        private readonly AdminOrderViewModel _order;
        private readonly IOrderService _orderService;
        private readonly IDialogService _dialogService;
        private readonly IMessageBus _messageBus;

        public StatusUpdateViewModel(
            AdminOrderViewModel order,
            IOrderService orderService,
            IDialogService dialogService,
            IMessageBus messageBus)
        {
            _order = order;
            _orderService = orderService;
            _dialogService = dialogService;
            _messageBus = messageBus;

            // Setup properties
            OrderCode = order.OrderCode;
            CurrentStatus = order.Status;

            // Setup available statuses for changing to
            AvailableStatuses = Enum.GetNames(typeof(OrderStatus))
                .Where(s => s != CurrentStatus)
                .ToList();

            if (AvailableStatuses.Any())
            {
                SelectedStatus = AvailableStatuses.First();
            }

            // Initialize command
            UpdateStatusCommand = new AsyncRelayCommand(UpdateStatusAsync);
            CancelCommand = new RelayCommand(Cancel);
        }

        public string OrderCode { get; }
        public string CurrentStatus { get; }

        private List<string> _availableStatuses;
        public List<string> AvailableStatuses
        {
            get => _availableStatuses;
            set => SetProperty(ref _availableStatuses, value);
        }

        private string _selectedStatus;
        public string SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
        }

        public ICommand UpdateStatusCommand { get; }
        public ICommand CancelCommand { get; }

        private bool _dialogResult = false;
        public bool DialogResult
        {
            get => _dialogResult;
            set => SetProperty(ref _dialogResult, value);
        }

        private async Task UpdateStatusAsync()
        {
            try
            {
                IsBusy = true;

                // Parse the new status
                var newStatus = (OrderStatus)Enum.Parse(typeof(OrderStatus), SelectedStatus);

                // Update the order status
                await _orderService.UpdateOrderStatusAsync(_order.Id, newStatus);

                // Update the order in the UI
                _order.Status = SelectedStatus;

                // Publish message to notify other parts of the app
                _messageBus.Publish(new OrderStatusChangedMessage
                {
                    OrderId = _order.Id,
                    NewStatus = SelectedStatus
                });

                // Set dialog result to true (success)
                DialogResult = true;

                // Close the dialog (will be handled in the code-behind)
                CloseDialog();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating order status: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
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
    }
}