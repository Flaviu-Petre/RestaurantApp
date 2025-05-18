using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantApp.UI.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        public RegisterViewModel(
            IUserService userService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _userService = userService;
            _dialogService = dialogService;
            _navigationService = navigationService;

            RegisterCommand = new AsyncRelayCommand(RegisterAsync);
            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
        }

        // Properties for form fields
        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        private string _address;
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        private bool _isEmployee;
        public bool IsEmployee
        {
            get => _isEmployee;
            set => SetProperty(ref _isEmployee, value);
        }

        // Commands
        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        // Methods
        private async Task RegisterAsync()
        {
            try
            {
                IsBusy = true;
                HasError = false;
                ErrorMessage = string.Empty;

                // Validate input
                if (string.IsNullOrWhiteSpace(FirstName))
                {
                    ErrorMessage = "First name is required";
                    HasError = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(LastName))
                {
                    ErrorMessage = "Last name is required";
                    HasError = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(Email))
                {
                    ErrorMessage = "Email is required";
                    HasError = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(PhoneNumber))
                {
                    ErrorMessage = "Phone number is required";
                    HasError = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(Address))
                {
                    ErrorMessage = "Address is required";
                    HasError = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Password is required";
                    HasError = true;
                    return;
                }

                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Passwords do not match";
                    HasError = true;
                    return;
                }

                // Check if email already exists
                bool emailExists = await _userService.EmailExistsAsync(Email);
                if (emailExists)
                {
                    ErrorMessage = $"A user with email {Email} already exists";
                    HasError = true;
                    return;
                }

                // Create user
                var user = new User
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    DeliveryAddress = Address,
                    Role = IsEmployee ? UserRole.Employee : UserRole.Customer
                };

                var createdUser = await _userService.CreateUserAsync(user, Password);

                if (createdUser != null)
                {
                    _dialogService.ShowMessage("Account created successfully. You can now log in.",
                        "Registration Successful", System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);

                    // Navigate to login view
                    _navigationService.NavigateTo("LoginView");
                }
                else
                {
                    ErrorMessage = "Failed to create account. Please try again.";
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration error: {ex.Message}";
                HasError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void NavigateToLogin()
        {
            _navigationService.NavigateTo("LoginView");
        }
    }
}