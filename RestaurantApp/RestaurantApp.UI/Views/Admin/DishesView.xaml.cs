using System.Windows.Controls;
using RestaurantApp.UI.ViewModels.Admin;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantApp.UI.Views.Admin
{
    public partial class DishesView : UserControl
    {
        public DishesView()
        {
            InitializeComponent();

            // Get required services
            var dishService = App.ServiceProvider.GetService<IDishService>();
            var categoryService = App.ServiceProvider.GetService<ICategoryService>();
            var allergenService = App.ServiceProvider.GetService<IAllergenService>();
            var dialogService = App.ServiceProvider.GetService<IDialogService>();
            var messageBus = App.ServiceProvider.GetService<IMessageBus>();

            // Create and set ViewModel
            if (dishService != null && categoryService != null && allergenService != null &&
                dialogService != null && messageBus != null)
            {
                DataContext = new DishManagementViewModel(
                    dishService,
                    categoryService,
                    allergenService,
                    dialogService,
                    messageBus);
            }
            else
            {
                MessageBox.Show("Failed to initialize DishesView: Required services not available.",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}