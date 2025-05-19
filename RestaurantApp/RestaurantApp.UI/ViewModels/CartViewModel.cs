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
    public class CartViewModel : ViewModelBase
    {
        private readonly ICartService _cartService;
        private readonly IUserSessionService _userSessionService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IOrderService _orderService;

        public CartViewModel(
            ICartService cartService,
            IUserSessionService userSessionService,
            INavigationService navigationService,
            IDialogService dialogService,
            IOrderService orderService)
        {
            _cartService = cartService;
            _userSessionService = userSessionService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _orderService = orderService;

            // Initialize commands
            IncreaseQuantityCommand = new RelayCommand<CartItem>(IncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand<CartItem>(DecreaseQuantity);
            RemoveFromCartCommand = new RelayCommand<CartItem>(RemoveFromCart);
            CheckoutCommand = new AsyncRelayCommand(CheckoutAsync, CanCheckout);

            // Load cart items
            RefreshCart();
        }

        // Properties
        private ObservableCollection<CartItem> _cartItems;
        public ObservableCollection<CartItem> CartItems
        {
            get => _cartItems;
            set => SetProperty(ref _cartItems, value);
        }

        public bool IsCartEmpty => !_cartService.HasItems();

        public decimal Subtotal => _cartService.GetSubtotal();

        public decimal ShippingCost => _cartService.GetShippingCost();

        public decimal Discount => _cartService.GetDiscount();

        public decimal Total => _cartService.GetTotal();

        public bool HasDiscount => Discount > 0;

        // Commands
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand CheckoutCommand { get; }

        // Methods
        private void RefreshCart()
        {
            var items = _cartService.GetCartItems();
            CartItems = new ObservableCollection<CartItem>(items);

            // Update dependent properties
            OnPropertyChanged(nameof(IsCartEmpty));
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(ShippingCost));
            OnPropertyChanged(nameof(Discount));
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(HasDiscount));
        }

        private void IncreaseQuantity(CartItem item)
        {
            if (item == null) return;

            _cartService.UpdateQuantity(item.Id, item.IsDish, item.Quantity + 1);
            RefreshCart();
        }

        private void DecreaseQuantity(CartItem item)
        {
            if (item == null) return;

            if (item.Quantity > 1)
            {
                _cartService.UpdateQuantity(item.Id, item.IsDish, item.Quantity - 1);
            }
            else
            {
                RemoveFromCart(item);
            }

            RefreshCart();
        }

        private void RemoveFromCart(CartItem item)
        {
            if (item == null) return;

            _cartService.RemoveItem(item.Id, item.IsDish);
            RefreshCart();
        }

        private async Task CheckoutAsync()
        {
            if (!_userSessionService.IsLoggedIn || !_userSessionService.IsCustomer)
            {
                _dialogService.ShowMessage("You need to be logged in as a customer to checkout.",
                    "Login Required", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                _navigationService.NavigateTo("LoginView");
                return;
            }

            try
            {
                IsBusy = true;

                // Create new order
                var order = new Order
                {
                    UserId = _userSessionService.CurrentUser.Id,
                    OrderDate = DateTime.Now,
                    ShippingCost = ShippingCost,
                    Discount = Discount,
                    FoodCost = Subtotal,
                    Status = OrderStatus.Registered,
                    EstimatedDeliveryTime = DateTime.Now.AddHours(1) // Delivery in approximately 1 hour
                };

                // Create order details from cart items
                var orderDetails = CartItems.Select(item => new OrderDetail
                {
                    DishId = item.IsDish ? item.Id : null,
                    MenuId = !item.IsDish ? item.Id : null,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList();

                // Create the order
                await _orderService.CreateOrderAsync(order, orderDetails);

                // Clear the cart
                _cartService.ClearCart();
                RefreshCart();

                // Show success message
                _dialogService.ShowMessage("Your order has been placed successfully!",
                    "Order Placed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                // Navigate to orders view
                _navigationService.NavigateTo("OrdersView");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error placing order: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanCheckout()
        {
            return _cartService.HasItems() && _userSessionService.IsLoggedIn && _userSessionService.IsCustomer;
        }
    }
}