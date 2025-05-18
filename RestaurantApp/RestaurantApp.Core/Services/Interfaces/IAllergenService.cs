using RestaurantApp.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Services.Interfaces
{
    public interface IAllergenService
    {
        Task<IEnumerable<Allergen>> GetAllAllergensAsync();
        Task<Allergen> GetAllergenByIdAsync(int id);
        Task<Allergen> GetAllergenWithDishesAsync(int id);
        Task<Allergen> CreateAllergenAsync(Allergen allergen);
        Task UpdateAllergenAsync(Allergen allergen);
        Task DeleteAllergenAsync(int id);
    }
}