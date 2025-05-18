using Microsoft.EntityFrameworkCore;
using RestaurantApp.Core.Models;
using RestaurantApp.Core.Interfaces.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApp.Data.Repositories.Implementations
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(RestaurantDbContext context) : base(context)
        {
        }

        public async Task<Category> GetWithDishesAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Dishes)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> GetWithMenusAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Menus)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> GetWithDishesAndMenusAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Dishes)
                .Include(c => c.Menus)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Category>> GetAllWithDishesAsync()
        {
            return await _context.Categories
                .Include(c => c.Dishes)
                .ToListAsync();
        }
    }
}