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
    public class AdminOrdersViewModel : ViewModelBase
    {
        private readonly IOrderService _orderService;
        private readonly IDialogService _dialogService;
        private readonly IMessageBus _messageBus;

        public AdminOrdersViewModel(
            IOrderService orderService,
            IDialogService dialogService,
            IMessageBus messageBus)
        {
            _orderService = orderService;
            _dialogService = dialogService;
            _messageBus = messageBus;

            // Initialize commands
            RefreshCommand = new AsyncRelayCommand(LoadOrdersAsync);
            ShowStatusUpdateDialogCommand = new RelayCommand<AdminOrderViewModel>(ShowStatusUpdateDialog);
            ViewOrderDetailsCommand = new RelayCommand<AdminOrderViewModel>(ViewOrderDetails);

            // Setup sorting and filtering options
            OrderStatuses = Enum.GetNames(typeof(OrderStatus)).ToList();
            OrderStatuses.Insert(0, "All");
            SelectedOrderStatus = "All";

            SortOptions = new List<string>
            {
                "Date (Newest First)",
                "Date (Oldest First)",
                "Status",
                "Total (High to Low)",
                "Total (Low to High)"
            };
            SelectedSortOption = SortOptions.First();

            // Load orders
            LoadOrdersAsync().ConfigureAwait(false);
        }

        #region Properties

        private ObservableCollection<AdminOrderViewModel> _orders;
        public ObservableCollection<AdminOrderViewModel> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        private ObservableCollection<AdminOrderViewModel> _filteredOrders;
        public ObservableCollection<AdminOrderViewModel> FilteredOrders
        {
            get => _filteredOrders;
            set => SetProperty(ref _filteredOrders, value);
        }

        private List<string> _orderStatuses;
        public List<string> OrderStatuses
        {
            get => _orderStatuses;
            set => SetProperty(ref _orderStatuses, value);
        }

        private string _selectedOrderStatus;
        public string SelectedOrderStatus
        {
            get => _selectedOrderStatus;
            set
            {
                if (SetProperty(ref _selectedOrderStatus, value))
                {
                    ApplyFiltersAndSort();
                }
            }
        }

        private List<string> _sortOptions;
        public List<string> SortOptions
        {
            get => _sortOptions;
            set => SetProperty(ref _sortOptions, value);
        }

        private string _selectedSortOption;
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (SetProperty(ref _selectedSortOption, value))
                {
                    ApplyFiltersAndSort();
                }
            }
        }

        private AdminOrderViewModel _selectedOrder;
        public AdminOrderViewModel SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand ShowStatusUpdateDialogCommand { get; }
        public ICommand ViewOrderDetailsCommand { get; }

        #endregion

        #region Methods

        private async Task LoadOrdersAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var orders = await _orderService.GetAllOrdersAsync();

                var orderViewModels = orders.Select(o => new AdminOrderViewModel
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
                    CustomerName = o.User != null ? $"{o.User.FirstName} {o.User.LastName}".Trim() : "Unknown Customer",
                    CustomerEmail = o.User?.Email,
                    CustomerPhone = o.User?.PhoneNumber,
                    DeliveryAddress = o.User?.DeliveryAddress,
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

                Orders = new ObservableCollection<AdminOrderViewModel>(orderViewModels);

                // Apply filters and sort
                ApplyFiltersAndSort();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading orders: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFiltersAndSort()
        {
            if (Orders == null)
                return;

            // Apply status filter
            var filtered = Orders.AsEnumerable();
            if (SelectedOrderStatus != "All")
            {
                filtered = filtered.Where(o => o.Status == SelectedOrderStatus);
            }

            // Apply sort
            filtered = SelectedSortOption switch
            {
                "Date (Newest First)" => filtered.OrderByDescending(o => o.OrderDate),
                "Date (Oldest First)" => filtered.OrderBy(o => o.OrderDate),
                "Status" => filtered.OrderBy(o => o.Status),
                "Total (High to Low)" => filtered.OrderByDescending(o => o.TotalCost),
                "Total (Low to High)" => filtered.OrderBy(o => o.TotalCost),
                _ => filtered.OrderByDescending(o => o.OrderDate), // Default sort
            };

            FilteredOrders = new ObservableCollection<AdminOrderViewModel>(filtered);
        }

        private void ShowStatusUpdateDialog(AdminOrderViewModel order)
        {
            if (order == null)
                return;

            try
            {
                // Create the dialog view model
                var statusUpdateViewModel = new StatusUpdateViewModel(
                    order,
                    _orderService,
                    _dialogService,
                    _messageBus);

                // Create the dialog window
                var dialog = new RestaurantApp.UI.Views.Dialogs.StatusUpdateDialog
                {
                    DataContext = statusUpdateViewModel,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                // Show the dialog and check the result
                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    // The dialog was closed with a successful result
                    // Refresh the orders list to show the updated status
                    LoadOrdersAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Error showing status update dialog: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void ViewOrderDetails(AdminOrderViewModel order)
        {
            if (order == null)
                return;

            // For now, just select the order to show details in the expanded view
            SelectedOrder = order;
        }

        #endregion
    }

    public class AdminOrderViewModel
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
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string DeliveryAddress { get; set; }
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