using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace RestaurantApp.Data
{
    public class RestaurantDbContextFactory : IDesignTimeDbContextFactory<RestaurantDbContext>
    {
        public RestaurantDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RestaurantDbContext>();
            optionsBuilder.UseSqlServer("Server=Localhost;Database=RestaurantAppDb;User ID=Petre_Flaviu;Password=3932;TrustServerCertificate=True;MultipleActiveResultSets=true");

            return new RestaurantDbContext(optionsBuilder.Options);
        }
    }
}