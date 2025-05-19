using System.Windows.Controls;
using RestaurantApp.UI.ViewModels;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantApp.UI.Views
{
    public partial class OrdersView : UserControl
    {
        public OrdersView()
        {
            InitializeComponent();

            // Get required services
            var orderService = App.ServiceProvider.GetService<IOrderService>();
            var userSessionService = App.ServiceProvider.GetService<IUserSessionService>();
            var dialogService = App.ServiceProvider.GetService<IDialogService>();
            var messageBus = App.ServiceProvider.GetService<IMessageBus>();

            // Create and set ViewModel
            if (orderService != null && userSessionService != null &&
                dialogService != null && messageBus != null)
            {
                DataContext = new OrdersViewModel(
                    orderService,
                    userSessionService,
                    dialogService,
                    messageBus);
            }
            else
            {
                MessageBox.Show("Failed to initialize OrdersView: Required services not available.",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}