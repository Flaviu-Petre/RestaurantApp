using Microsoft.EntityFrameworkCore;
using RestaurantApp.Core.Models;
using RestaurantApp.Core.Interfaces.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApp.Data.Repositories.Implementations
{
    public class AllergenRepository : Repository<Allergen>, IAllergenRepository
    {
        public AllergenRepository(RestaurantDbContext context) : base(context)
        {
        }

        public async Task<Allergen> GetWithDishesAsync(int id)
        {
            return await _context.Allergens
                .Include(a => a.DishAllergens)
                    .ThenInclude(da => da.Dish)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Allergen>> GetAllWithDishesAsync()
        {
            return await _context.Allergens
                .Include(a => a.DishAllergens)
                    .ThenInclude(da => da.Dish)
                .ToListAsync();
        }
    }
}