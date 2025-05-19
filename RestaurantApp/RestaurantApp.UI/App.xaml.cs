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
using Microsoft.EntityFrameworkCore.Design;

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

                // Register DbContext with TRANSIENT lifetime
                string connectionString = configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<RestaurantDbContext>(options =>
                    options.UseSqlServer(connectionString), ServiceLifetime.Transient);

                // Register StoredProcedureExecutor as transient
                services.AddTransient<IStoredProcedureExecutor, StoredProcedureExecutor>();

                // Register DbContextFactory
                services.AddSingleton<IDesignTimeDbContextFactory<RestaurantDbContext>>(
                    new RestaurantDbContextFactory(connectionString));

                // Register repositories as transient
                services.AddTransient<IRepositoryFactory, RepositoryFactory>();
                services.AddTransient<ICategoryRepository, CategoryRepository>();
                services.AddTransient<IDishRepository, DishRepository>();
                services.AddTransient<IMenuRepository, MenuRepository>();
                services.AddTransient<IOrderRepository, OrderRepository>();
                services.AddTransient<IUserRepository, UserRepository>();
                services.AddTransient<IAllergenRepository, AllergenRepository>();

                // Register business services as transient
                services.AddTransient<ICategoryService, CategoryService>();
                services.AddTransient<IDishService, DishService>();
                services.AddTransient<IMenuService, MenuService>();
                services.AddTransient<IAllergenService, AllergenService>();
                services.AddTransient<IOrderService, OrderService>();
                services.AddTransient<IUserService, UserService>();
                services.AddTransient<IAuthenticationService, AuthenticationService>();
                services.AddTransient<IConfigurationService, ConfigurationService>();

                // Register UI infrastructure services that should be singletons
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
                navigationService.RegisterView("MenuManagementView", typeof(MenuManagementView));

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