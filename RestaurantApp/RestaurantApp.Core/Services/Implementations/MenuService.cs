using RestaurantApp.Core.Interfaces.Repositories;
using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.Core.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Services.Implementations
{
    public class MenuService : BaseService, IMenuService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IDishRepository _dishRepository;
        private readonly IConfigurationService _configurationService;

        public MenuService(
            IRepositoryFactory repositoryFactory,
            IConfigurationService configurationService) : base(repositoryFactory)
        {
            _menuRepository = repositoryFactory.Menus;
            _dishRepository = repositoryFactory.Dishes;
            _configurationService = configurationService;
        }

        public async Task<IEnumerable<Menu>> GetAllMenusAsync()
        {
            return await _menuRepository.GetAllAsync();
        }

        public async Task<Menu> GetMenuByIdAsync(int id)
        {
            return await _menuRepository.GetByIdAsync(id);
        }

        public async Task<Menu> GetMenuWithDetailsAsync(int id)
        {
            return await _menuRepository.GetWithFullDetailsAsync(id);
        }

        public async Task<IEnumerable<Menu>> SearchMenusByNameAsync(string searchTerm)
        {
            return await _menuRepository.SearchByNameAsync(searchTerm);
        }

        public async Task<IEnumerable<Menu>> SearchMenusByAllergenAsync(int allergenId, bool includeAllergen)
        {
            return await _menuRepository.SearchByAllergenAsync(allergenId, includeAllergen);
        }

        public async Task<decimal> CalculateMenuPriceAsync(int menuId, decimal discountPercentage)
        {
            var menu = await _menuRepository.GetWithDishesAsync(menuId);
            if (menu == null)
                throw new InvalidOperationException($"Menu with ID {menuId} not found");

            // If no discount provided, get from configuration
            if (discountPercentage <= 0)
            {
                discountPercentage = await _configurationService.GetMenuDiscountPercentageAsync();
            }

            return menu.CalculatePrice(discountPercentage);
        }

        public async Task<Menu> CreateMenuAsync(Menu menu, IEnumerable<MenuDish> menuDishes)
        {
            if (menu == null)
                throw new ArgumentNullException(nameof(menu));

            // First save the menu
            var createdMenu = await _menuRepository.AddAsync(menu);

            // Then add dishes to the menu
            if (menuDishes != null && menuDishes.Any())
            {
                foreach (var menuDish in menuDishes)
                {
                    menuDish.MenuId = createdMenu.Id;
                }

                // Update the menu with dishes
                createdMenu.MenuDishes = menuDishes.ToList();
                await _menuRepository.UpdateAsync(createdMenu);
            }

            return createdMenu;
        }

        public async Task UpdateMenuAsync(Menu menu, IEnumerable<MenuDish> menuDishes)
        {
            if (menu == null)
                throw new ArgumentNullException(nameof(menu));

            // Get existing menu with its dishes
            var existingMenu = await _menuRepository.GetWithDishesAsync(menu.Id);
            if (existingMenu == null)
                throw new InvalidOperationException($"Menu with ID {menu.Id} not found");

            // Update basic menu properties
            existingMenu.Name = menu.Name;
            existingMenu.Description = menu.Description;
            existingMenu.CategoryId = menu.CategoryId;

            // Replace the dishes
            if (menuDishes != null)
            {
                existingMenu.MenuDishes.Clear();
                foreach (var menuDish in menuDishes)
                {
                    menuDish.MenuId = existingMenu.Id;
                    existingMenu.MenuDishes.Add(menuDish);
                }
            }

            await _menuRepository.UpdateAsync(existingMenu);
        }

        public async Task DeleteMenuAsync(int id)
        {
            var menu = await _menuRepository.GetByIdAsync(id);
            if (menu == null)
                throw new InvalidOperationException($"Menu with ID {id} not found");

            await _menuRepository.DeleteAsync(menu);
        }

        public async Task<bool> IsMenuAvailableAsync(int id)
        {
            var menu = await _menuRepository.GetWithDishesAsync(id);
            if (menu == null)
                return false;

            // Check if all dishes in the menu are available
            foreach (var menuDish in menu.MenuDishes)
            {
                var dish = await _dishRepository.GetByIdAsync(menuDish.DishId);
                if (dish == null || dish.TotalQuantity <= 0)
                    return false;
            }

            return true;
        }
    }
}