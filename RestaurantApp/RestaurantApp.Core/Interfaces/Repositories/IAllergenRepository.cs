using RestaurantApp.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Interfaces.Repositories
{
    public interface IAllergenRepository : IRepository<Allergen>
    {
        // Get allergen with related dishes
        Task<Allergen> GetWithDishesAsync(int id);

        // Get all allergens with their associated dishes
        Task<IEnumerable<Allergen>> GetAllWithDishesAsync();
    }
}