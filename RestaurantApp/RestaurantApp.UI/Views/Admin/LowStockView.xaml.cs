using System.Windows.Controls;
using RestaurantApp.UI.ViewModels.Admin;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantApp.UI.Views.Admin
{
    /// <summary>
    /// Interaction logic for LowStockView.xaml
    /// </summary>
    public partial class LowStockView : UserControl
    {
        public LowStockView()
        {
            InitializeComponent();

            // Get required services
            var dishService = App.ServiceProvider.GetService<IDishService>();
            var configService = App.ServiceProvider.GetService<IConfigurationService>();
            var dialogService = App.ServiceProvider.GetService<IDialogService>();

            // Create and set ViewModel
            if (dishService != null && configService != null && dialogService != null)
            {
                DataContext = new LowStockViewModel(
                    dishService,
                    configService,
                    dialogService);
            }
            else
            {
                MessageBox.Show("Failed to initialize LowStockView: Required services not available.",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}