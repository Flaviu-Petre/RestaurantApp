using Microsoft.EntityFrameworkCore;
using RestaurantApp.Core.Models;
using RestaurantApp.Core.Interfaces.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantApp.Data.Repositories.Implementations
{
    public class DishRepository : Repository<Dish>, IDishRepository
    {
        public DishRepository(RestaurantDbContext context) : base(context)
        {
        }

        public async Task<Dish> GetWithCategoryAsync(int id)
        {
            return await _context.Dishes
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Dish> GetWithAllergensAsync(int id)
        {
            return await _context.Dishes
                .Include(d => d.DishAllergens)
                    .ThenInclude(da => da.Allergen)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Dish> GetWithImagesAsync(int id)
        {
            return await _context.Dishes
                .Include(d => d.Images)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Dish> GetWithMenusAsync(int id)
        {
            return await _context.Dishes
                .Include(d => d.MenuDishes)
                    .ThenInclude(md => md.Menu)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Dish> GetWithFullDetailsAsync(int id)
        {
            return await _context.Dishes
                .Include(d => d.Category)
                .Include(d => d.Images)
                .Include(d => d.DishAllergens)
                    .ThenInclude(da => da.Allergen)
                .Include(d => d.MenuDishes)
                    .ThenInclude(md => md.Menu)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Dish>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _context.Dishes
                .Include(d => d.Category)
                .Include(d => d.Images)
                .Include(d => d.DishAllergens)
                    .ThenInclude(da => da.Allergen)
                .Where(d => d.Name.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<IEnumerable<Dish>> SearchByAllergenAsync(int allergenId, bool includeAllergen)
        {
            var query = _context.Dishes
                .Include(d => d.Category)
                .Include(d => d.Images)
                .Include(d => d.DishAllergens)
                    .ThenInclude(da => da.Allergen);

            if (includeAllergen)
            {
                // Dishes that contain the allergen
                return await query
                    .Where(d => d.DishAllergens.Any(da => da.AllergenId == allergenId))
                    .ToListAsync();
            }
            else
            {
                // Dishes that do NOT contain the allergen
                return await query
                    .Where(d => !d.DishAllergens.Any(da => da.AllergenId == allergenId))
                    .ToListAsync();
            }
        }

        public async Task<IEnumerable<Dish>> GetLowStockDishesAsync(decimal threshold)
        {
            return await _context.Dishes
                .Include(d => d.Category)
                .Where(d => d.TotalQuantity <= threshold)
                .OrderBy(d => d.TotalQuantity)
                .ToListAsync();
        }

        public async Task UpdateTotalQuantityAsync(int id, decimal newQuantity)
        {
            var dish = await GetByIdAsync(id);
            if (dish != null)
            {
                dish.TotalQuantity = newQuantity;
                await UpdateAsync(dish);
            }
        }
    }
}