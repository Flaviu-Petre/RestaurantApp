using System.Windows.Controls;
using RestaurantApp.UI.ViewModels.Admin;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantApp.UI.Views.Admin
{
    /// <summary>
    /// Interaction logic for ManageOrdersView.xaml
    /// </summary>
    public partial class ManageOrdersView : UserControl
    {
        public ManageOrdersView()
        {
            InitializeComponent();

            // Get required services
            var orderService = App.ServiceProvider.GetService<IOrderService>();
            var dialogService = App.ServiceProvider.GetService<IDialogService>();
            var messageBus = App.ServiceProvider.GetService<IMessageBus>();

            // Create and set ViewModel
            if (orderService != null && dialogService != null && messageBus != null)
            {
                DataContext = new AdminOrdersViewModel(
                    orderService,
                    dialogService,
                    messageBus);
            }
            else
            {
                MessageBox.Show("Failed to initialize ManageOrdersView: Required services not available.",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}