using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace RestaurantApp.Data
{
    public static class DatabaseConfiguration
    {
        public static void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                // Use default connection string from configuration file if none provided
                connectionString = GetConnectionStringFromConfiguration();
            }

            optionsBuilder.UseSqlServer(connectionString);
        }

        private static string GetConnectionStringFromConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();
            return configuration.GetConnectionString("DefaultConnection");
        }
    }
}