using System.Windows.Controls;
using RestaurantApp.UI.ViewModels;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantApp.UI.Views
{
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            InitializeComponent();

            // Get required services from App.ServiceProvider
            var userService = App.ServiceProvider.GetService<IUserService>();
            var dialogService = App.ServiceProvider.GetService<IDialogService>();
            var navigationService = App.ServiceProvider.GetService<INavigationService>();

            // Create and set ViewModel
            if (userService != null && dialogService != null && navigationService != null)
            {
                DataContext = new RegisterViewModel(userService, dialogService, navigationService);
            }
            else
            {
                MessageBox.Show("Failed to initialize RegisterView: Required services not available.",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}