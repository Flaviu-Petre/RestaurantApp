using RestaurantApp.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Services.Interfaces
{
    public interface IDishService
    {
        Task<IEnumerable<Dish>> GetAllDishesAsync();
        Task<Dish> GetDishByIdAsync(int id);
        Task<Dish> GetDishWithDetailsAsync(int id);
        Task<IEnumerable<Dish>> SearchDishesByNameAsync(string searchTerm);
        Task<IEnumerable<Dish>> SearchDishesByAllergenAsync(int allergenId, bool includeAllergen);
        Task<IEnumerable<Dish>> GetLowStockDishesAsync(decimal threshold);
        Task<Dish> CreateDishAsync(Dish dish);
        Task UpdateDishAsync(Dish dish);
        Task DeleteDishAsync(int id);
        Task UpdateDishQuantityAsync(int id, decimal newQuantity);
        Task<bool> IsDishAvailableAsync(int id, decimal requiredQuantity);
    }
}