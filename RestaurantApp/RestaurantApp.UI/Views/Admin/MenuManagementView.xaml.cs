using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using RestaurantApp.UI.ViewModels.Admin;

namespace RestaurantApp.UI.Views.Admin
{
    public partial class MenuManagementView : UserControl
    {
        public MenuManagementView()
        {
            InitializeComponent();

            try
            {
                // Get required services
                var menuService = App.ServiceProvider.GetService<IMenuService>();
                var categoryService = App.ServiceProvider.GetService<ICategoryService>();
                var dishService = App.ServiceProvider.GetService<IDishService>();
                var configService = App.ServiceProvider.GetService<IConfigurationService>();
                var dialogService = App.ServiceProvider.GetService<IDialogService>();
                var messageBus = App.ServiceProvider.GetService<IMessageBus>();

                // Create and set ViewModel
                if (menuService != null && categoryService != null && dishService != null &&
                    configService != null && dialogService != null && messageBus != null)
                {
                    DataContext = new MenuManagementViewModel(
                        menuService,
                        categoryService,
                        dishService,
                        configService,
                        dialogService,
                        messageBus);
                }
                else
                {
                    MessageBox.Show("Failed to initialize MenuManagementView: Required services not available.",
                        "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error initializing MenuManagementView: {ex.Message}",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}