using System.Windows.Controls;
using RestaurantApp.UI.ViewModels;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantApp.UI.Views
{
    public partial class CartView : UserControl
    {
        public CartView()
        {
            InitializeComponent();

            // Get required services
            var cartService = App.ServiceProvider.GetService<ICartService>();
            var userSessionService = App.ServiceProvider.GetService<IUserSessionService>();
            var navigationService = App.ServiceProvider.GetService<INavigationService>();
            var dialogService = App.ServiceProvider.GetService<IDialogService>();
            var orderService = App.ServiceProvider.GetService<IOrderService>();

            // Create and set ViewModel
            if (cartService != null && userSessionService != null && navigationService != null &&
                dialogService != null && orderService != null)
            {
                DataContext = new CartViewModel(
                    cartService,
                    userSessionService,
                    navigationService,
                    dialogService,
                    orderService);
            }
            else
            {
                MessageBox.Show("Failed to initialize CartView: Required services not available.",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}