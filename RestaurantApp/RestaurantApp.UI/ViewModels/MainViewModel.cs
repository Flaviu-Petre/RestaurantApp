using RestaurantApp.Core.Models;
using RestaurantApp.UI.Infrastructure;
using System;
using System.Windows.Input;

namespace RestaurantApp.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IUserSessionService _userSessionService;
        private readonly IMessageBus _messageBus;
        private ViewModelBase _currentView;

        public MainViewModel(
            INavigationService navigationService,
            IUserSessionService userSessionService,
            IMessageBus messageBus)
        {
            _navigationService = navigationService;
            _userSessionService = userSessionService;
            _messageBus = messageBus;

            // Subscribe to login/logout events
            _userSessionService.UserLoggedIn += OnUserLoggedIn;
            _userSessionService.UserLoggedOut += OnUserLoggedOut;

            // Initialize commands
            NavigateToMenuCommand = new RelayCommand(NavigateToMenu);
            NavigateToSearchCommand = new RelayCommand(NavigateToSearch);
            NavigateToOrdersCommand = new RelayCommand(NavigateToOrders);
            NavigateToCartCommand = new RelayCommand(NavigateToCart);
            NavigateToRegisterCommand = new RelayCommand(NavigateToRegister);
            NavigateToCategoriesCommand = new RelayCommand(NavigateToCategories);
            NavigateToDishesCommand = new RelayCommand(NavigateToDishes);
            NavigateToMenusCommand = new RelayCommand(NavigateToMenus);
            NavigateToAllergensCommand = new RelayCommand(NavigateToAllergens);
            NavigateToAllOrdersCommand = new RelayCommand(NavigateToAllOrders);
            NavigateToLowStockCommand = new RelayCommand(NavigateToLowStock);
            LoginCommand = new RelayCommand(Login);

            // Initial view - navigate to menu by default
            NavigateToMenu();
        }

        // Session properties
        public bool IsLoggedIn => _userSessionService.IsLoggedIn;
        public bool IsCustomer => _userSessionService.IsCustomer;
        public bool IsEmployee => _userSessionService.IsEmployee;

        public string WelcomeMessage => IsLoggedIn
            ? $"Welcome, {_userSessionService.CurrentUser?.FirstName}"
            : "Welcome, Guest";

        public string LoginButtonText => IsLoggedIn ? "Logout" : "Login";

        // Current view
        public ViewModelBase CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        // Status message
        private string _statusMessage = "Ready";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // Navigation commands
        public ICommand NavigateToMenuCommand { get; }
        public ICommand NavigateToSearchCommand { get; }
        public ICommand NavigateToOrdersCommand { get; }
        public ICommand NavigateToCartCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand NavigateToCategoriesCommand { get; }
        public ICommand NavigateToDishesCommand { get; }
        public ICommand NavigateToMenusCommand { get; }
        public ICommand NavigateToAllergensCommand { get; }
        public ICommand NavigateToAllOrdersCommand { get; }
        public ICommand NavigateToLowStockCommand { get; }
        public ICommand LoginCommand { get; }

        // Navigation methods
        private void NavigateToMenu()
        {
            _navigationService.NavigateTo("MenuView");
            StatusMessage = "Browsing menu";
        }

        private void NavigateToSearch()
        {
            _navigationService.NavigateTo("SearchView");
            StatusMessage = "Searching menu items";
        }

        private void NavigateToOrders()
        {
            if (EnsureLoggedIn())
            {
                _navigationService.NavigateTo("OrdersView");
                StatusMessage = "Viewing your orders";
            }
        }

        private void NavigateToCart()
        {
            if (EnsureLoggedIn())
            {
                _navigationService.NavigateTo("CartView");
                StatusMessage = "Viewing your cart";
            }
        }

        private void NavigateToRegister()
        {
            _navigationService.NavigateTo("RegisterView");
            StatusMessage = "Create a new account";
        }

        private void NavigateToCategories()
        {
            if (EnsureEmployee())
            {
                _navigationService.NavigateTo("CategoriesView");
                StatusMessage = "Managing categories";
            }
        }

        private void NavigateToDishes()
        {
            if (EnsureEmployee())
            {
                _navigationService.NavigateTo("DishesView");
                StatusMessage = "Managing dishes";
            }
        }

        private void NavigateToMenus()
        {
            if (EnsureEmployee())
            {
                _navigationService.NavigateTo("MenusView");
                StatusMessage = "Managing menus";
            }
        }

        private void NavigateToAllergens()
        {
            if (EnsureEmployee())
            {
                _navigationService.NavigateTo("AllergensView");
                StatusMessage = "Managing allergens";
            }
        }

        private void NavigateToAllOrders()
        {
            if (EnsureEmployee())
            {
                _navigationService.NavigateTo("AllOrdersView");
                StatusMessage = "Managing orders";
            }
        }

        private void NavigateToLowStock()
        {
            if (EnsureEmployee())
            {
                _navigationService.NavigateTo("LowStockView");
                StatusMessage = "Viewing low stock items";
            }
        }

        private void Login()
        {
            if (IsLoggedIn)
            {
                // Logout
                _userSessionService.Logout();
                NavigateToMenu();
            }
            else
            {
                // Navigate to login
                _navigationService.NavigateTo("LoginView");
                StatusMessage = "Please log in";
            }
        }

        // Helper methods
        private bool EnsureLoggedIn()
        {
            if (!IsLoggedIn)
            {
                _navigationService.NavigateTo("LoginView");
                StatusMessage = "Please log in to continue";
                return false;
            }
            return true;
        }

        private bool EnsureEmployee()
        {
            if (!IsLoggedIn || !IsEmployee)
            {
                // Either not logged in or not an employee
                if (!IsLoggedIn)
                {
                    _navigationService.NavigateTo("LoginView");
                    StatusMessage = "Please log in to continue";
                }
                else
                {
                    NavigateToMenu();
                    StatusMessage = "Access denied. Employee access required.";
                }
                return false;
            }
            return true;
        }

        // Event handlers
        private void OnUserLoggedIn(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsLoggedIn));
            OnPropertyChanged(nameof(IsCustomer));
            OnPropertyChanged(nameof(IsEmployee));
            OnPropertyChanged(nameof(WelcomeMessage));
            OnPropertyChanged(nameof(LoginButtonText));

            // If employee, go to all orders by default, otherwise go to menu
            if (IsEmployee)
                NavigateToAllOrders();
            else
                NavigateToMenu();
        }

        private void OnUserLoggedOut(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsLoggedIn));
            OnPropertyChanged(nameof(IsCustomer));
            OnPropertyChanged(nameof(IsEmployee));
            OnPropertyChanged(nameof(WelcomeMessage));
            OnPropertyChanged(nameof(LoginButtonText));

            // Navigate to menu after logout
            NavigateToMenu();
        }

        public override void Cleanup()
        {
            _userSessionService.UserLoggedIn -= OnUserLoggedIn;
            _userSessionService.UserLoggedOut -= OnUserLoggedOut;
            base.Cleanup();
        }
    }
}