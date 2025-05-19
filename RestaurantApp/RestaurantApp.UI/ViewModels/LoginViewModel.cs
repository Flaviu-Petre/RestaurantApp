using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System;
using System.Windows.Input;

namespace RestaurantApp.UI.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authService;
        private readonly IUserService _userService;
        private readonly IUserSessionService _userSessionService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        public LoginViewModel(
            IAuthenticationService authService,
            IUserService userService,
            IUserSessionService userSessionService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _authService = authService;
            _userService = userService;
            _userSessionService = userSessionService;
            _navigationService = navigationService;
            _dialogService = dialogService;

            LoginCommand = new AsyncRelayCommand(LoginAsync);
            NavigateToRegisterCommand = new RelayCommand(NavigateToRegister);
        }

        // Properties
        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        // Commands
        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        // Methods
        private async Task LoginAsync()
        {
            try
            {
                IsBusy = true;
                HasError = false;
                ErrorMessage = string.Empty;

                // Validate input
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Please enter both email and password";
                    HasError = true;
                    return;
                }

                // Authenticate user
                var user = await _authService.AuthenticateAsync(Email, Password);

                if (user != null)
                {
                    // Clear login form
                    Email = string.Empty;
                    Password = string.Empty;

                    // Navigate to appropriate view based on user role first
                    if (user.Role == Core.Models.UserRole.Employee)
                    {
                        _navigationService.NavigateTo("AllOrdersView");
                    }
                    else
                    {
                        _navigationService.NavigateTo("MenuView");
                    }

                    // Then login the user - this will trigger the UserChanged event
                    _userSessionService.Login(user);
                }
                else
                {
                    // Login failed
                    ErrorMessage = "Invalid email or password";
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login error: {ex.Message}";
                HasError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void NavigateToRegister()
        {
            _navigationService.NavigateTo("RegisterView");
        }
    }
}