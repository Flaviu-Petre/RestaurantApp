using Microsoft.EntityFrameworkCore;
using System;

namespace RestaurantApp.Data
{
    public interface IDbContextProvider
    {
        RestaurantDbContext GetContext();
    }

    public class DbContextProvider : IDbContextProvider
    {
        private readonly string _connectionString;

        public DbContextProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public RestaurantDbContext GetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);

            return new RestaurantDbContext(optionsBuilder.Options);
        }
    }
}