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
                List<Menu> menus = null;

                // Create separate tasks for each data fetch operation to isolate DbContext usage
                await Task.Run(async () =>
                {
                    try
                    {
                        // First operation
                        categories = (await _categoryService.GetAllCategoriesAsync()).ToList();
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = $"Error loading categories: {ex.Message}";
                    }
                });

                if (categories != null)
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            // Second operation - only if first succeeded
                            dishes = (await _dishService.GetAllDishesAsync()).ToList();
                        }
                        catch (Exception ex)
                        {
                            ErrorMessage = $"Error loading dishes: {ex.Message}";
                        }
                    });
                }

                if (dishes != null)
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            // Third operation - only if second succeeded
                            menus = (await _menuService.GetAllMenusAsync()).ToList();
                        }
                        catch (Exception ex)
                        {
                            ErrorMessage = $"Error loading menus: {ex.Message}";
                        }
                    });
                }

                // Only proceed with UI updates if we have all the data
                if (categories != null && dishes != null && menus != null)
                {
                    // Process data and update UI on the UI thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var categoryViewModels = categories.Select(c => new CategoryViewModel
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Description = c.Description
                        }).ToList();

                        var dishViewModels = dishes.Select(d => CreateDishViewModel(d)).ToList();
                        var menuViewModels = menus.Select(m => CreateMenuViewModel(m)).ToList();

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
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading menu: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
            return new MenuItemViewModel
            {
                Id = menu.Id,
                Name = menu.Name,
                Description = menu.Description ?? "Delicious menu", // Default if null
                Price = menu.CalculatePrice(10), // Calculate locally instead of using service
                PortionQuantity = 0, // Menu doesn't have a single portion quantity
                CategoryId = menu.CategoryId,
                ImageUrl = "/Images/default-menu.png", // Default menu image
                AllergensList = GetMenuAllergensList(menu),
                HasAllergens = DoesMenuHaveAllergens(menu),
                IsAvailable = IsMenuAvailable(menu),
                IsMenu = true // Indicate this is a menu, not a dish
            };
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
            try
            {
                // Use a separate task to avoid DbContext sharing
                var task = Task.Run(async () => await _menuService.CalculateMenuPriceAsync(menu.Id, 10));
                task.Wait(); // Since we're already on a background thread, we can wait
                return task.Result;
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