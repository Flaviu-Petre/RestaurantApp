using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace RestaurantApp.Data
{
    // Add this class to your Data project
    public class RestaurantDbContextFactory : IDesignTimeDbContextFactory<RestaurantDbContext>
    {
        private readonly string _connectionString;

        public RestaurantDbContextFactory()
        {
            _connectionString = "Server=Localhost;Database=RestaurantAppDb;User ID=Petre_Flaviu;Password=3932;TrustServerCertificate=True;MultipleActiveResultSets=true";
        }

        public RestaurantDbContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public RestaurantDbContext CreateDbContext(string[] args = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            return new RestaurantDbContext(optionsBuilder.Options);
        }
    }
}