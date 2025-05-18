using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantApp.UI.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IDishService _dishService;
        private readonly IMenuService _menuService;
        private readonly IUserSessionService _userSessionService;
        private readonly ICartService _cartService;
        private readonly IDialogService _dialogService;

        public MenuViewModel(
            ICategoryService categoryService,
            IDishService dishService,
            IMenuService menuService,
            IUserSessionService userSessionService,
            ICartService cartService,
            IDialogService dialogService)
        {
            _categoryService = categoryService;
            _dishService = dishService;
            _menuService = menuService;
            _userSessionService = userSessionService;
            _cartService = cartService;
            _dialogService = dialogService;

            // Initialize commands
            AddToCartCommand = new RelayCommand<MenuItemViewModel>(AddToCart);
            RefreshCommand = new RelayCommand(async () => await LoadDataAsync());

            // Load data
            _ = LoadDataAsync();
        }

        // Properties
        private ObservableCollection<IGrouping<CategoryViewModel, MenuItemViewModel>> _categoryGroups;
        public ObservableCollection<IGrouping<CategoryViewModel, MenuItemViewModel>> CategoryGroups
        {
            get => _categoryGroups;
            set => SetProperty(ref _categoryGroups, value);
        }

        private bool _hasItems;
        public bool HasItems
        {
            get => _hasItems;
            set => SetProperty(ref _hasItems, value);
        }

        public bool IsCustomer => _userSessionService.IsCustomer;

        // Commands
        public ICommand AddToCartCommand { get; }
        public ICommand RefreshCommand { get; }

        // Methods
        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;

                // Load categories, dishes, and menus
                var categories = await _categoryService.GetAllCategoriesAsync();
                var dishes = await _dishService.GetAllDishesAsync();
                var menus = await _menuService.GetAllMenusAsync();

                // Create category view models
                var categoryViewModels = categories.Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList();

                // Create dish view models
                var dishViewModels = dishes.Select(d => new DishItemViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = "Delicious dish", // Default description since Dish doesn't have Description property
                    Price = d.Price,
                    PortionQuantity = d.PortionQuantity,
                    CategoryId = d.CategoryId,
                    ImageUrl = GetDishImageUrl(d),
                    AllergensList = GetDishAllergensList(d),
                    HasAllergens = (d.DishAllergens?.Any() ?? false),
                    IsAvailable = d.TotalQuantity > 0
                }).ToList();

                // Create menu view models
                var menuViewModels = menus.Select(m => new MenuItemViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description ?? "Delicious menu", // Default if null
                    Price = CalculateMenuPrice(m),
                    PortionQuantity = 0, // Menu doesn't have a single portion quantity
                    CategoryId = m.CategoryId,
                    ImageUrl = "/Images/default-menu.png", // Default menu image
                    AllergensList = GetMenuAllergensList(m),
                    HasAllergens = DoesMenuHaveAllergens(m),
                    IsAvailable = IsMenuAvailable(m),
                    IsMenu = true // Indicate this is a menu, not a dish
                }).ToList();

                // Combine dish and menu items
                var menuItems = new List<MenuItemViewModel>();
                menuItems.AddRange(dishViewModels);
                menuItems.AddRange(menuViewModels);

                // Group by category
                var categoryGroups = menuItems
                    .GroupBy(item => categoryViewModels.FirstOrDefault(c => c.Id == item.CategoryId))
                    .Where(g => g.Key != null) // Filter out any items without a category
                    .ToList();

                CategoryGroups = new ObservableCollection<IGrouping<CategoryViewModel, MenuItemViewModel>>(categoryGroups);
                HasItems = categoryGroups.Any() && categoryGroups.SelectMany(g => g).Any();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading menu: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Helper methods for Dish properties
        private string GetDishImageUrl(Dish dish)
        {
            if (dish.Images != null && dish.Images.Any())
            {
                var firstImage = dish.Images.FirstOrDefault();
                return firstImage?.ImagePath ?? "/Images/default-dish.png";
            }
            return "/Images/default-dish.png";
        }

        private string GetDishAllergensList(Dish dish)
        {
            if (dish.DishAllergens == null || !dish.DishAllergens.Any())
                return "";

            return string.Join(", ", dish.DishAllergens
                .Where(da => da.Allergen != null)
                .Select(da => da.Allergen.Name));
        }

        // Helper methods for Menu properties
        private decimal CalculateMenuPrice(Menu menu)
        {
            // You can either call the menu service or calculate it directly
            try
            {
                return _menuService.CalculateMenuPriceAsync(menu.Id, 10).Result; // 10% is a placeholder discount
            }
            catch
            {
                // Fallback if service call fails
                return menu.CalculatePrice(10); // 10% discount
            }
        }

        private string GetMenuAllergensList(Menu menu)
        {
            if (menu.MenuDishes == null || !menu.MenuDishes.Any())
                return "";

            var allergens = menu.MenuDishes
                .Where(md => md.Dish?.DishAllergens != null)
                .SelectMany(md => md.Dish.DishAllergens)
                .Where(da => da.Allergen != null)
                .Select(da => da.Allergen.Name)
                .Distinct();

            return string.Join(", ", allergens);
        }

        private bool DoesMenuHaveAllergens(Menu menu)
        {
            return menu.MenuDishes?.Any(md => md.Dish?.DishAllergens?.Any() ?? false) ?? false;
        }

        private bool IsMenuAvailable(Menu menu)
        {
            return menu.MenuDishes?.All(md => md.Dish?.TotalQuantity > 0) ?? false;
        }

        private void AddToCart(MenuItemViewModel item)
        {
            if (item == null || !_userSessionService.IsLoggedIn || !IsCustomer)
                return;

            try
            {
                if (!item.IsMenu) // It's a dish
                {
                    _cartService.AddDish(item.Id, 1);
                }
                else // It's a menu
                {
                    _cartService.AddMenu(item.Id, 1);
                }

                _dialogService.ShowMessage($"{item.Name} added to cart", "Item Added", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding to cart: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }

    // View model classes for menu items
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class MenuItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal PortionQuantity { get; set; }
        public int CategoryId { get; set; }
        public string ImageUrl { get; set; }
        public string AllergensList { get; set; }
        public bool HasAllergens { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsMenu { get; set; } // Indicates if this is a menu or dish
    }

    public class DishItemViewModel : MenuItemViewModel
    {
        public DishItemViewModel()
        {
            IsMenu = false; // This is a dish, not a menu
        }
    }
}