// In RestaurantApp.Core/Interfaces/Data/IStoredProcedureExecutor.cs
using RestaurantApp.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Interfaces.Data
{
    public interface IStoredProcedureExecutor
    {
        Task<List<Dish>> SearchDishesByNameAsync(string searchTerm);
        Task<List<Dish>> SearchDishesByAllergenAsync(int allergenId, bool includeAllergen);
        Task<List<Dish>> GetPopularDishesAsync(int topCount);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
        Task CreateStoredProceduresAsync();
        
    }
}