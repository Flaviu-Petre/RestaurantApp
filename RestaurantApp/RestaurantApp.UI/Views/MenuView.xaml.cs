using System.Windows.Controls;
using RestaurantApp.UI.ViewModels;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantApp.UI.Views
{
    public partial class MenuView : UserControl
    {
        public MenuView()
        {
            InitializeComponent();

            // Using Task.Run to offload service retrieval and ViewModel initialization from UI thread
            Task.Run(() =>
            {
                try
                {
                    // Get required services
                    var categoryService = App.ServiceProvider.GetService<ICategoryService>();
                    var dishService = App.ServiceProvider.GetService<IDishService>();
                    var menuService = App.ServiceProvider.GetService<IMenuService>();
                    var userSessionService = App.ServiceProvider.GetService<IUserSessionService>();
                    var cartService = App.ServiceProvider.GetService<ICartService>();
                    var dialogService = App.ServiceProvider.GetService<IDialogService>();

                    // Create the ViewModel
                    if (categoryService != null && dishService != null && menuService != null &&
                        userSessionService != null && cartService != null && dialogService != null)
                    {
                        var viewModel = new MenuViewModel(
                            categoryService,
                            dishService,
                            menuService,
                            userSessionService,
                            cartService,
                            dialogService);

                        // Update UI on the main thread
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            DataContext = viewModel;
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("Failed to initialize MenuView: Required services not available.",
                                "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Error initializing MenuView: {ex.Message}",
                            "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            });
        }
    }
}