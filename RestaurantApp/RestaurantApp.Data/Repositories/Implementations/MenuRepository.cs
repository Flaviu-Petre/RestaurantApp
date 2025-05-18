using Microsoft.EntityFrameworkCore;
using RestaurantApp.Core.Models;
using RestaurantApp.Core.Interfaces.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantApp.Data.Repositories.Implementations
{
    public class MenuRepository : Repository<Menu>, IMenuRepository
    {
        public MenuRepository(RestaurantDbContext context) : base(context)
        {
        }

        public async Task<Menu> GetWithCategoryAsync(int id)
        {
            return await _context.Menus
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Menu> GetWithDishesAsync(int id)
        {
            return await _context.Menus
                .Include(m => m.MenuDishes)
                    .ThenInclude(md => md.Dish)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Menu> GetWithFullDetailsAsync(int id)
        {
            return await _context.Menus
                .Include(m => m.Category)
                .Include(m => m.MenuDishes)
                    .ThenInclude(md => md.Dish)
                        .ThenInclude(d => d.Images)
                .Include(m => m.MenuDishes)
                    .ThenInclude(md => md.Dish)
                        .ThenInclude(d => d.DishAllergens)
                            .ThenInclude(da => da.Allergen)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Menu>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _context.Menus
                .Include(m => m.Category)
                .Include(m => m.MenuDishes)
                    .ThenInclude(md => md.Dish)
                .Where(m => m.Name.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<IEnumerable<Menu>> SearchByAllergenAsync(int allergenId, bool includeAllergen)
        {
            var query = _context.Menus
                .Include(m => m.Category)
                .Include(m => m.MenuDishes)
                    .ThenInclude(md => md.Dish)
                        .ThenInclude(d => d.DishAllergens)
                            .ThenInclude(da => da.Allergen);

            if (includeAllergen)
            {
                // Menus that contain dishes with the allergen
                return await query
                    .Where(m => m.MenuDishes.Any(md => md.Dish.DishAllergens.Any(da => da.AllergenId == allergenId)))
                    .ToListAsync();
            }
            else
            {
                // Menus that do NOT contain any dishes with the allergen
                return await query
                    .Where(m => !m.MenuDishes.Any(md => md.Dish.DishAllergens.Any(da => da.AllergenId == allergenId)))
                    .ToListAsync();
            }
        }
    }
}