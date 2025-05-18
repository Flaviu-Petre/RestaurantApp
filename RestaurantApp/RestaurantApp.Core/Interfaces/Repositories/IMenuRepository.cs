using RestaurantApp.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Interfaces.Repositories
{
    public interface IMenuRepository : IRepository<Menu>
    {
        // Get menu with related entities
        Task<Menu> GetWithCategoryAsync(int id);
        Task<Menu> GetWithDishesAsync(int id);
        Task<Menu> GetWithFullDetailsAsync(int id);

        // Search menus
        Task<IEnumerable<Menu>> SearchByNameAsync(string searchTerm);
        Task<IEnumerable<Menu>> SearchByAllergenAsync(int allergenId, bool includeAllergen);
    }
}