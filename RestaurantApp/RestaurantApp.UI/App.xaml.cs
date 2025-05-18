using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantApp.Core.Interfaces.Data;
using RestaurantApp.Data;
using RestaurantApp.Data.Repositories.Implementations;
using RestaurantApp.Core.Interfaces.Repositories;
using RestaurantApp.UI.Infrastructure;
using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using RestaurantApp.Core.Services.Implementations;
using RestaurantApp.Core.Services.Interfaces;
using System.IO;

namespace RestaurantApp.UI
{
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();

            // Initialize database and apply migrations
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
                dbContext.Database.Migrate();
            }

            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            // Get connection string
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            // Register DbContext
            services.AddDbContext<RestaurantDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Register StoredProcedureExecutor
            services.AddScoped<StoredProcedureExecutor>();

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

            // Register MVVM infrastructure
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IUserSessionService, UserSessionService>();
            services.AddSingleton<IMessageBus, MessageBus>();

            // Register ViewModels
            //services.AddTransient<MainViewModel>();

            // Register MainWindow
            services.AddTransient<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}