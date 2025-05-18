using RestaurantApp.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Services.Interfaces
{
    public interface IMenuService
    {
        Task<IEnumerable<Menu>> GetAllMenusAsync();
        Task<Menu> GetMenuByIdAsync(int id);
        Task<Menu> GetMenuWithDetailsAsync(int id);
        Task<IEnumerable<Menu>> SearchMenusByNameAsync(string searchTerm);
        Task<IEnumerable<Menu>> SearchMenusByAllergenAsync(int allergenId, bool includeAllergen);
        Task<decimal> CalculateMenuPriceAsync(int menuId, decimal discountPercentage);
        Task<Menu> CreateMenuAsync(Menu menu, IEnumerable<MenuDish> menuDishes);
        Task UpdateMenuAsync(Menu menu, IEnumerable<MenuDish> menuDishes);
        Task DeleteMenuAsync(int id);
        Task<bool> IsMenuAvailableAsync(int id);
    }
}