using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantApp.UI.ViewModels
{
    public class OrdersViewModel : ViewModelBase
    {
        private readonly IOrderService _orderService;
        private readonly IUserSessionService _userSessionService;
        private readonly IDialogService _dialogService;
        private readonly IMessageBus _messageBus;

        public OrdersViewModel(
            IOrderService orderService,
            IUserSessionService userSessionService,
            IDialogService dialogService,
            IMessageBus messageBus)
        {
            _orderService = orderService;
            _userSessionService = userSessionService;
            _dialogService = dialogService;
            _messageBus = messageBus;

            // Initialize commands
            CancelOrderCommand = new AsyncRelayCommand<OrderViewModel>(CancelOrderAsync);
            RefreshCommand = new AsyncRelayCommand(LoadOrdersAsync);

            // Subscribe to message bus for order status changes
            _messageBus.Subscribe<OrderStatusChangedMessage>(this, OnOrderStatusChanged);

            // Load orders
            LoadOrdersAsync().ConfigureAwait(false);
        }

        // Properties
        private ObservableCollection<OrderViewModel> _orders;
        public ObservableCollection<OrderViewModel> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        public bool HasOrders => Orders != null && Orders.Any();

        // Commands
        public ICommand CancelOrderCommand { get; }
        public ICommand RefreshCommand { get; }

        // Methods
        private async Task LoadOrdersAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                if (!_userSessionService.IsLoggedIn)
                {
                    Orders = new ObservableCollection<OrderViewModel>();
                    OnPropertyChanged(nameof(HasOrders));
                    return;
                }

                var userId = _userSessionService.CurrentUser.Id;
                var orders = await _orderService.GetUserOrdersAsync(userId);

                var orderViewModels = orders.Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    OrderDate = o.OrderDate,
                    Status = o.Status.ToString(),
                    FoodCost = o.FoodCost,
                    ShippingCost = o.ShippingCost,
                    Discount = o.Discount,
                    TotalCost = o.TotalCost,
                    EstimatedDeliveryTime = o.EstimatedDeliveryTime,
                    DeliveryAddress = _userSessionService.CurrentUser.DeliveryAddress,
                    CanCancel = o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled,
                    OrderDetails = o.OrderDetails?.Select(od => new OrderDetailViewModel
                    {
                        ItemName = od.DishId.HasValue
                            ? od.Dish?.Name ?? "Unknown Dish"
                            : od.Menu?.Name ?? "Unknown Menu",
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        TotalPrice = od.Quantity * od.UnitPrice
                    }).ToList()
                }).ToList();

                Orders = new ObservableCollection<OrderViewModel>(orderViewModels);
                OnPropertyChanged(nameof(HasOrders));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading orders: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CancelOrderAsync(OrderViewModel order)
        {
            if (order == null || !order.CanCancel)
                return;

            var result = _dialogService.ShowMessage($"Are you sure you want to cancel order {order.OrderCode}?",
                "Confirm Cancellation", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    await _orderService.CancelOrderAsync(order.Id);

                    // Update the order in the UI
                    order.Status = OrderStatus.Cancelled.ToString();
                    order.CanCancel = false;

                    _dialogService.ShowMessage("Order has been cancelled.", "Order Cancelled",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                    // Publish message to notify others
                    _messageBus.Publish(new OrderStatusChangedMessage
                    {
                        OrderId = order.Id,
                        NewStatus = OrderStatus.Cancelled.ToString()
                    });
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error cancelling order: {ex.Message}";
                    _dialogService.ShowMessage(ErrorMessage, "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private void OnOrderStatusChanged(OrderStatusChangedMessage message)
        {
            // Update order status in the UI if it exists in the collection
            var order = Orders?.FirstOrDefault(o => o.Id == message.OrderId);
            if (order != null)
            {
                order.Status = message.NewStatus;
                order.CanCancel = message.NewStatus != OrderStatus.Delivered.ToString() &&
                                  message.NewStatus != OrderStatus.Cancelled.ToString();
            }
        }

        public override void Cleanup()
        {
            _messageBus.Unsubscribe<OrderStatusChangedMessage>(this);
            base.Cleanup();
        }
    }

    public class OrderViewModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal FoodCost { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public string DeliveryAddress { get; set; }
        public bool CanCancel { get; set; }
        public ICollection<OrderDetailViewModel> OrderDetails { get; set; }
    }

    public class OrderDetailViewModel
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}