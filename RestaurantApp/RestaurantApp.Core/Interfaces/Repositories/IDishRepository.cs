using RestaurantApp.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Interfaces.Repositories
{
    public interface IDishRepository : IRepository<Dish>
    {
        // Get dish with related entities
        Task<Dish> GetWithCategoryAsync(int id);
        Task<Dish> GetWithAllergensAsync(int id);
        Task<Dish> GetWithImagesAsync(int id);
        Task<Dish> GetWithMenusAsync(int id);
        Task<Dish> GetWithFullDetailsAsync(int id);
        Task<IEnumerable<Dish>> GetAllWithFullDetailsAsync();

        // Search dishes
        Task<IEnumerable<Dish>> SearchByNameAsync(string searchTerm);
        Task<IEnumerable<Dish>> SearchByAllergenAsync(int allergenId, bool includeAllergen);
        Task<IEnumerable<Dish>> GetLowStockDishesAsync(decimal threshold);

        // Update specific properties
        Task UpdateTotalQuantityAsync(int id, decimal newQuantity);
    }
}