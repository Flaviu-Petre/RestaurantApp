using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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

            // Subscribe to user session changes
            _userSessionService.UserChanged += OnUserChanged;

            // Initialize commands
            AddToCartCommand = new RelayCommand<MenuItemViewModel>(AddToCart);
            RefreshCommand = new RelayCommand(async () => await LoadDataAsync());

            // Load data asynchronously
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

        // Expose user type for visibility binding
        public bool IsCustomer => _userSessionService.IsCustomer;

        // Commands
        public ICommand AddToCartCommand { get; }
        public ICommand RefreshCommand { get; }

        // Methods
        private void OnUserChanged(object sender, EventArgs e)
        {
            // Notify UI that IsCustomer property has changed
            OnPropertyChanged(nameof(IsCustomer));

            // Reload data - this is important!
            _ = LoadDataAsync();
        }

        public override void Cleanup()
        {
            _userSessionService.UserChanged -= OnUserChanged;
            base.Cleanup();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                // Use separate tasks to isolate DbContext operations
                List<Category> categories = null;
                List<Dish> dishes = null;
                List<Menu> menusWithDetails = null;

                // Load categories
                await Task.Run(async () =>
                {
                    try
                    {
                        categories = (await _categoryService.GetAllCategoriesAsync()).ToList();
                        System.Diagnostics.Debug.WriteLine($"Loaded {categories.Count} categories");
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = $"Error loading categories: {ex.Message}";
                        System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
                    }
                });

                // Load dishes
                if (categories != null)
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            dishes = (await _dishService.GetAllDishesAsync()).ToList();
                            System.Diagnostics.Debug.WriteLine($"Loaded {dishes.Count} dishes");
                        }
                        catch (Exception ex)
                        {
                            ErrorMessage = $"Error loading dishes: {ex.Message}";
                            System.Diagnostics.Debug.WriteLine($"Error loading dishes: {ex.Message}");
                        }
                    });
                }

                // Load menus WITH DETAILS
                if (dishes != null)
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            // First get all menu IDs
                            var basicMenus = (await _menuService.GetAllMenusAsync()).ToList();
                            System.Diagnostics.Debug.WriteLine($"Found {basicMenus.Count} menus");

                            // Then load each menu with full details
                            menusWithDetails = new List<Menu>();
                            foreach (var menu in basicMenus)
                            {
                                try
                                {
                                    var detailedMenu = await _menuService.GetMenuWithDetailsAsync(menu.Id);
                                    if (detailedMenu != null)
                                    {
                                        // Verify dishes are loaded
                                        if (detailedMenu.MenuDishes != null)
                                        {
                                            System.Diagnostics.Debug.WriteLine($"Menu {detailedMenu.Name} has {detailedMenu.MenuDishes.Count} dishes");

                                            // Verify each dish in menu dishes is loaded
                                            foreach (var menuDish in detailedMenu.MenuDishes)
                                            {
                                                if (menuDish.Dish == null)
                                                {
                                                    // If dish reference isn't loaded, find it from the dishes we already loaded
                                                    var dish = dishes.FirstOrDefault(d => d.Id == menuDish.DishId);
                                                    if (dish != null)
                                                    {
                                                        menuDish.Dish = dish;
                                                        System.Diagnostics.Debug.WriteLine($"Manually linked dish {dish.Name} to menu");
                                                    }
                                                    else
                                                    {
                                                        System.Diagnostics.Debug.WriteLine($"Could not find dish with ID {menuDish.DishId}");
                                                    }
                                                }
                                                else
                                                {
                                                    System.Diagnostics.Debug.WriteLine($"Menu includes dish: {menuDish.Dish.Name} with price {menuDish.Dish.Price}");
                                                }
                                            }
                                        }

                                        menusWithDetails.Add(detailedMenu);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error loading details for menu {menu.Id}: {ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorMessage = $"Error loading menus: {ex.Message}";
                            System.Diagnostics.Debug.WriteLine($"Error loading menus: {ex.Message}");
                        }
                    });
                }

                // Only proceed with UI updates if we have all the data
                if (categories != null && dishes != null && menusWithDetails != null)
                {
                    // Process data and update UI on the UI thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Convert categories to view models
                        var categoryViewModels = categories.Select(c => new CategoryViewModel
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Description = c.Description
                        }).ToList();

                        // Convert dishes to view models
                        var dishViewModels = dishes.Select(d => CreateDishViewModel(d)).ToList();

                        // Convert menus to view models using our detailed menus
                        var menuViewModels = menusWithDetails.Select(m => CreateMenuViewModel(m)).ToList();

                        // Log menu prices for debugging
                        foreach (var menuVM in menuViewModels)
                        {
                            System.Diagnostics.Debug.WriteLine($"Menu in view model: {menuVM.Name}, Price: {menuVM.Price}");
                        }

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

                        System.Diagnostics.Debug.WriteLine($"Created {CategoryGroups.Count} category groups with {menuItems.Count} total items");
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading menu: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error in LoadDataAsync: {ex.Message}");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _dialogService.ShowMessage(ErrorMessage, "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private DishItemViewModel CreateDishViewModel(Dish dish)
        {
            return new DishItemViewModel
            {
                Id = dish.Id,
                Name = dish.Name,
                Description = "Delicious dish", // Default description
                Price = dish.Price,
                PortionQuantity = dish.PortionQuantity,
                CategoryId = dish.CategoryId,
                ImageUrl = GetDishImageUrl(dish),
                AllergensList = GetDishAllergensList(dish),
                HasAllergens = (dish.DishAllergens?.Any() ?? false),
                IsAvailable = dish.TotalQuantity > 0
            };
        }
        private MenuItemViewModel CreateMenuViewModel(Menu menu)
        {
            System.Diagnostics.Debug.WriteLine($"Creating menu view model for: {menu.Name}");

            // Default discount percentage
            decimal discountPercentage = 10m;

            // Initialize price and portion quantity
            decimal menuPrice = 0;
            decimal totalPortionQuantity = 0;

            // Check if MenuDishes collection is loaded and contains items
            if (menu.MenuDishes != null && menu.MenuDishes.Any())
            {
                decimal totalDishesPrice = 0;

                foreach (var menuDish in menu.MenuDishes)
                {
                    if (menuDish.Dish != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Adding dish price: {menuDish.Dish.Name} = {menuDish.Dish.Price}");
                        totalDishesPrice += menuDish.Dish.Price;

                        // Accumulate portion quantity 
                        if (menuDish.QuantityInMenu > 0)
                        {
                            totalPortionQuantity += menuDish.QuantityInMenu;
                        }
                        else
                        {
                            totalPortionQuantity += menuDish.Dish.PortionQuantity;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"  Dish not loaded for MenuDish with DishId: {menuDish.DishId}");
                    }
                }

                // Apply discount to price
                //menuPrice = totalDishesPrice * (1 - (discountPercentage / 100m));
                menuPrice = totalDishesPrice;
                System.Diagnostics.Debug.WriteLine($"  Calculated price: {menuPrice} (from total: {totalDishesPrice})");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"  Menu doesn't have dishes loaded");

                // Use menu's built-in calculation method
                try
                {
                    menuPrice = menu.CalculatePrice(discountPercentage);
                    System.Diagnostics.Debug.WriteLine($"  Used menu's calculation method: {menuPrice}");

                    // Default portion quantity since we can't calculate it
                    totalPortionQuantity = 200;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"  Error calculating menu price: {ex.Message}");
                    menuPrice = 0;
                    totalPortionQuantity = 0;
                }
            }

            // Round the portion quantity to avoid excessive decimal places
            totalPortionQuantity = Math.Round(totalPortionQuantity);

            // Create and return the view model
            var viewModel = new MenuItemViewModel
            {
                Id = menu.Id,
                Name = menu.Name,
                Description = menu.Description ?? "Delicious menu",
                Price = menuPrice,
                PortionQuantity = totalPortionQuantity,
                CategoryId = menu.CategoryId,
                ImageUrl = "/Images/default-menu.png",
                AllergensList = GetMenuAllergensList(menu),
                HasAllergens = DoesMenuHaveAllergens(menu),
                IsAvailable = IsMenuAvailable(menu),
                IsMenu = true
            };

            System.Diagnostics.Debug.WriteLine($"  Created view model with price: {viewModel.Price}");
            return viewModel;
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

        // Fix the AddToCart method in MenuViewModel.cs
        private void AddToCart(MenuItemViewModel item)
        {
            if (item == null || !_userSessionService.IsLoggedIn || !_userSessionService.IsCustomer)
            {
                if (!_userSessionService.IsLoggedIn)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _dialogService.ShowMessage("You need to be logged in to add items to cart",
                            "Login Required", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    });
                    return;
                }
                return;
            }

            try
            {
                if (!item.IsAvailable)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _dialogService.ShowMessage("This item is currently not available",
                            "Unavailable", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    });
                    return;
                }

                if (!item.IsMenu) // It's a dish
                {
                    _cartService.AddDish(item.Id, 1);
                }
                else // It's a menu
                {
                    _cartService.AddMenu(item.Id, 1);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _dialogService.ShowMessage($"{item.Name} added to cart", "Item Added",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ErrorMessage = $"Error adding to cart: {ex.Message}";
                    _dialogService.ShowMessage(ErrorMessage, "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                });
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