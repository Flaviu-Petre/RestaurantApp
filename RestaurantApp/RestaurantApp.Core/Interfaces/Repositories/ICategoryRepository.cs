using RestaurantApp.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Interfaces.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        // Get category with related entities
        Task<Category> GetWithDishesAsync(int id);
        Task<Category> GetWithMenusAsync(int id);
        Task<Category> GetWithDishesAndMenusAsync(int id);

        // Get all categories with related entities
        Task<IEnumerable<Category>> GetAllWithDishesAsync();
    }
}