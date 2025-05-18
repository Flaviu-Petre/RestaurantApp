using RestaurantApp.Core.Interfaces.Repositories;
using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Services.Implementations
{
    public class AllergenService : BaseService, IAllergenService
    {
        private readonly IAllergenRepository _allergenRepository;

        public AllergenService(IRepositoryFactory repositoryFactory) : base(repositoryFactory)
        {
            _allergenRepository = repositoryFactory.Allergens;
        }

        public async Task<IEnumerable<Allergen>> GetAllAllergensAsync()
        {
            return await _allergenRepository.GetAllAsync();
        }

        public async Task<Allergen> GetAllergenByIdAsync(int id)
        {
            return await _allergenRepository.GetByIdAsync(id);
        }

        public async Task<Allergen> GetAllergenWithDishesAsync(int id)
        {
            return await _allergenRepository.GetWithDishesAsync(id);
        }

        public async Task<Allergen> CreateAllergenAsync(Allergen allergen)
        {
            if (allergen == null)
                throw new ArgumentNullException(nameof(allergen));

            return await _allergenRepository.AddAsync(allergen);
        }

        public async Task UpdateAllergenAsync(Allergen allergen)
        {
            if (allergen == null)
                throw new ArgumentNullException(nameof(allergen));

            await _allergenRepository.UpdateAsync(allergen);
        }

        public async Task DeleteAllergenAsync(int id)
        {
            var allergen = await _allergenRepository.GetByIdAsync(id);
            if (allergen == null)
                throw new InvalidOperationException($"Allergen with ID {id} not found");

            await _allergenRepository.DeleteAsync(allergen);
        }
    }
}