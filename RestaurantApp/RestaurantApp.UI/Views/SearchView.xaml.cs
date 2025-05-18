using System.Windows.Controls;
using RestaurantApp.UI.ViewModels;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantApp.UI.Views
{
    public partial class SearchView : UserControl
    {
        public SearchView()
        {
            InitializeComponent();

            // Get required services
            var dishService = App.ServiceProvider.GetService<IDishService>();
            var menuService = App.ServiceProvider.GetService<IMenuService>();
            var allergenService = App.ServiceProvider.GetService<IAllergenService>();
            var userSessionService = App.ServiceProvider.GetService<IUserSessionService>();
            var cartService = App.ServiceProvider.GetService<ICartService>();
            var dialogService = App.ServiceProvider.GetService<IDialogService>();

            // Create and set ViewModel
            if (dishService != null && menuService != null && allergenService != null &&
                userSessionService != null && cartService != null && dialogService != null)
            {
                DataContext = new SearchViewModel(
                    dishService,
                    menuService,
                    allergenService,
                    userSessionService,
                    cartService,
                    dialogService);
            }
            else
            {
                MessageBox.Show("Failed to initialize SearchView: Required services not available.",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}