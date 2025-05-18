using System.Windows.Controls;
using RestaurantApp.UI.ViewModels.Admin;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantApp.UI.Views.Admin
{
    public partial class AllergensView : UserControl
    {
        public AllergensView()
        {
            InitializeComponent();

            // Get required services
            var allergenService = App.ServiceProvider.GetService<IAllergenService>();
            var dialogService = App.ServiceProvider.GetService<IDialogService>();
            var messageBus = App.ServiceProvider.GetService<IMessageBus>();

            // Create and set ViewModel
            if (allergenService != null && dialogService != null && messageBus != null)
            {
                DataContext = new AllergenManagementViewModel(allergenService, dialogService, messageBus);
            }
            else
            {
                MessageBox.Show("Failed to initialize AllergensView: Required services not available.",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}