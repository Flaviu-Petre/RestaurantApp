using RestaurantApp.Core.Interfaces.Repositories;
using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.Core.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Services.Implementations
{
    public class DishService : BaseService, IDishService
    {
        private readonly IDishRepository _dishRepository;
        private readonly IStoredProcedureExecutor _storedProcedureExecutor;

        public DishService(
            IRepositoryFactory repositoryFactory,
            IStoredProcedureExecutor storedProcedureExecutor) : base(repositoryFactory)
        {
            _dishRepository = repositoryFactory.Dishes;
            _storedProcedureExecutor = storedProcedureExecutor;
        }

        public async Task<IEnumerable<Dish>> GetAllDishesAsync()
        {
            return await _dishRepository.GetAllAsync();
        }

        public async Task<Dish> GetDishByIdAsync(int id)
        {
            return await _dishRepository.GetByIdAsync(id);
        }

        public async Task<Dish> GetDishWithDetailsAsync(int id)
        {
            return await _dishRepository.GetWithFullDetailsAsync(id);
        }

        public async Task<IEnumerable<Dish>> SearchDishesByNameAsync(string searchTerm)
        {
            // Using stored procedure
            return await _storedProcedureExecutor.SearchDishesByNameAsync(searchTerm);
        }

        public async Task<IEnumerable<Dish>> SearchDishesByAllergenAsync(int allergenId, bool includeAllergen)
        {
            // Using stored procedure
            return await _storedProcedureExecutor.SearchDishesByAllergenAsync(allergenId, includeAllergen);
        }

        public async Task<IEnumerable<Dish>> GetLowStockDishesAsync(decimal threshold)
        {
            return await _dishRepository.GetLowStockDishesAsync(threshold);
        }

        public async Task<Dish> CreateDishAsync(Dish dish)
        {
            if (dish == null)
                throw new ArgumentNullException(nameof(dish));

            return await _dishRepository.AddAsync(dish);
        }

        public async Task UpdateDishAsync(Dish dish)
        {
            if (dish == null)
                throw new ArgumentNullException(nameof(dish));

            await _dishRepository.UpdateAsync(dish);
        }

        public async Task DeleteDishAsync(int id)
        {
            var dish = await _dishRepository.GetByIdAsync(id);
            if (dish == null)
                throw new InvalidOperationException($"Dish with ID {id} not found");

            await _dishRepository.DeleteAsync(dish);
        }

        public async Task UpdateDishQuantityAsync(int id, decimal newQuantity)
        {
            await _dishRepository.UpdateTotalQuantityAsync(id, newQuantity);
        }

        public async Task<bool> IsDishAvailableAsync(int id, decimal requiredQuantity)
        {
            var dish = await _dishRepository.GetByIdAsync(id);
            if (dish == null)
                return false;

            return dish.TotalQuantity >= requiredQuantity;
        }
    }
}