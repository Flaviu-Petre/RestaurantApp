using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantApp.Core.Interfaces.Data;
using RestaurantApp.Data;
using RestaurantApp.Data.Repositories.Implementations;
using RestaurantApp.Core.Interfaces.Repositories;
using RestaurantApp.UI.Infrastructure;
using RestaurantApp.UI.ViewModels;
using RestaurantApp.UI.Views.Admin;
using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using RestaurantApp.Core.Services.Implementations;
using RestaurantApp.Core.Services.Interfaces;
using System.IO;
using RestaurantApp.UI.Views;

namespace RestaurantApp.UI
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Create a service collection
                var services = new ServiceCollection();

                // Get configuration from appsettings.json
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                services.AddSingleton<IConfiguration>(configuration);

                // Register DbContext
                string connectionString = configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<RestaurantDbContext>(options =>
                    options.UseSqlServer(connectionString));

                // Register StoredProcedureExecutor
                services.AddScoped<IStoredProcedureExecutor, StoredProcedureExecutor>();

                // Register repositories
                services.AddScoped<IRepositoryFactory, RepositoryFactory>();
                services.AddScoped<ICategoryRepository, CategoryRepository>();
                services.AddScoped<IDishRepository, DishRepository>();
                services.AddScoped<IMenuRepository, MenuRepository>();
                services.AddScoped<IOrderRepository, OrderRepository>();
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IAllergenRepository, AllergenRepository>();

                // Register services
                services.AddScoped<ICategoryService, CategoryService>();
                services.AddScoped<IDishService, DishService>();
                services.AddScoped<IMenuService, MenuService>();
                services.AddScoped<IAllergenService, AllergenService>();
                services.AddScoped<IOrderService, OrderService>();
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IAuthenticationService, AuthenticationService>();
                services.AddScoped<IConfigurationService, ConfigurationService>();

                // Register UI infrastructure services
                services.AddSingleton<IUserSessionService, UserSessionService>();
                services.AddSingleton<IMessageBus, MessageBus>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<ICartService, CartService>();

                // Build the service provider
                ServiceProvider = services.BuildServiceProvider();

                // Create the main window
                var mainWindow = new MainWindow();

                // Create and configure the navigation service
                var navigationService = new NavigationService(mainWindow.MainContent);

                // Register views
                navigationService.RegisterView("RegisterView", typeof(RegisterView));
                navigationService.RegisterView("MenuView", typeof(MenuView));
                navigationService.RegisterView("LoginView", typeof(LoginView));
                navigationService.RegisterView("SearchView", typeof(SearchView));
                navigationService.RegisterView("CartView", typeof(CartView));
                navigationService.RegisterView("OrdersView", typeof(OrdersView));
                navigationService.RegisterView("CategoriesView", typeof(CategoriesView));
                navigationService.RegisterView("DishesView", typeof(DishesView));
                navigationService.RegisterView("LowStockView", typeof(LowStockView));
                navigationService.RegisterView("AllOrdersView", typeof(ManageOrdersView));
                navigationService.RegisterView("AllergensView", typeof(AllergensView));
                navigationService.RegisterView("SearchView", typeof(SearchView));

                // Register the NavigationService as a singleton
                services.AddSingleton<INavigationService>(navigationService);

                // Rebuild the service provider to include NavigationService
                ServiceProvider = services.BuildServiceProvider();

                // Create the main view model with dependencies
                var mainViewModel = new MainViewModel(
                    navigationService,
                    ServiceProvider.GetRequiredService<IUserSessionService>(),
                    ServiceProvider.GetRequiredService<IMessageBus>());

                // Set the DataContext
                mainWindow.DataContext = mainViewModel;

                // Show the window
                mainWindow.Show();

                // Initialize the database if needed
                InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during startup: {ex.Message}\n\n{ex.StackTrace}",
                    "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                using (var scope = ServiceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
                    dbContext.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database initialization error: {ex.Message}",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}