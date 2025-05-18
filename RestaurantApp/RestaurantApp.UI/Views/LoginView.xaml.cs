using System.Windows.Controls;
using RestaurantApp.UI.ViewModels;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantApp.UI.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();

            // Get required services from App.ServiceProvider
            var authService = App.ServiceProvider.GetService<IAuthenticationService>();
            var userService = App.ServiceProvider.GetService<IUserService>();
            var userSessionService = App.ServiceProvider.GetService<IUserSessionService>();
            var navigationService = App.ServiceProvider.GetService<INavigationService>();
            var dialogService = App.ServiceProvider.GetService<IDialogService>();

            // Create and set ViewModel
            if (authService != null && userService != null && userSessionService != null &&
                navigationService != null && dialogService != null)
            {
                DataContext = new LoginViewModel(
                    authService,
                    userService,
                    userSessionService,
                    navigationService,
                    dialogService);
            }
            else
            {
                MessageBox.Show("Failed to initialize LoginView: Required services not available.",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}