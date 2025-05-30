﻿using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Implementations;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            AddToCartCommand = new RelayCommand<MenuItemViewModel>(AddToCart, item => item != null);
            RefreshCommand = new RelayCommand(async () => await LoadDataAsync());
            ShowItemDetailsCommand = new RelayCommand<MenuItemViewModel>(ShowItemDetails);

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
        public ICommand ShowItemDetailsCommand { get; }

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
            // Ensure dish.Category isn't null
            string categoryName = dish.Category?.Name ?? "Unknown Category";

            // Get dish description
            string description = string.IsNullOrEmpty(dish.Name) ?
                $"Delicious {dish.Name}" : dish.Name;

            // Get allergens list
            string allergensList = GetDishAllergensList(dish);

            // Create image path that works in the UI
            string imagePath = GetDishImageUrl(dish);

            // Create an image source that will work in the UI
            var imageSource = ImageHelper.LoadImageSource(imagePath);

            return new DishItemViewModel
            {
                Id = dish.Id,
                Name = dish.Name,
                Description = description,
                Price = dish.Price,
                PortionQuantity = dish.PortionQuantity,
                CategoryId = dish.CategoryId,
                CategoryName = categoryName,
                ImageUrl = imagePath,
                ImageSource = imageSource, // Add this property to your viewmodel
                AllergensList = allergensList,
                HasAllergens = !string.IsNullOrEmpty(allergensList),
                IsAvailable = dish.TotalQuantity > 0
            };
        }

        private string GetDishAllergensList(Dish dish)
        {
            if (dish.DishAllergens == null || !dish.DishAllergens.Any())
                return "";

            // Extrageți numele alergenilor
            var allergenNames = dish.DishAllergens
                .Where(da => da.Allergen != null)
                .Select(da => da.Allergen.Name)
                .ToList();

            return string.Join(", ", allergenNames);
        }


        private MenuItemViewModel CreateMenuViewModel(Menu menu)
        {
            System.Diagnostics.Debug.WriteLine($"Creating menu view model for: {menu.Name}");

            // Default discount percentage
            decimal discountPercentage = 10m;

            // Initialize price and portion quantity
            decimal menuPrice = 0;
            decimal totalPortionQuantity = 0;

            // Lista pentru a păstra dish-urile din meniu
            var menuItems = new List<MenuDishViewModel>();

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

                        // Adaugă dish-ul în lista de dish-uri din meniu
                        menuItems.Add(new MenuDishViewModel
                        {
                            DishId = menuDish.DishId,
                            DishName = menuDish.Dish.Name,
                            Quantity = menuDish.QuantityInMenu > 0 ? menuDish.QuantityInMenu : menuDish.Dish.PortionQuantity,
                            Unit = "g"
                        });

                        System.Diagnostics.Debug.WriteLine($"  Added menu dish: {menuDish.Dish.Name}, Quantity: {menuDish.QuantityInMenu}g");
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

            // Debug informații despre lista de dish-uri
            System.Diagnostics.Debug.WriteLine($"  Menu has {menuItems.Count} dishes");
            foreach (var item in menuItems)
            {
                System.Diagnostics.Debug.WriteLine($"    Dish in menu: {item.DishName} - {item.Quantity}{item.Unit}");
            }

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
                IsMenu = true,
                MenuItems = menuItems // Adăugăm lista de dish-uri
            };

            System.Diagnostics.Debug.WriteLine($"  Created view model with price: {viewModel.Price} and {viewModel.MenuItems.Count} dishes");
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
            // Run the operation on a background thread to avoid UI freezing
            Task.Run(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"AddToCart called for item: {item?.Name}");

                    if (item == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Item is null, returning");
                        return;
                    }

                    if (!_userSessionService.IsLoggedIn || !_userSessionService.IsCustomer)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _dialogService.ShowMessage("You need to be logged in to add items to cart",
                                "Login Required", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        });
                        return;
                    }

                    if (!item.IsAvailable)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _dialogService.ShowMessage("This item is currently not available",
                                "Unavailable", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        });
                        return;
                    }

                    // Add the item based on type
                    if (!item.IsMenu) // It's a dish
                    {
                        System.Diagnostics.Debug.WriteLine($"Adding dish to cart: {item.Id}");
                        _cartService.AddDish(item.Id, 1);
                    }
                    else // It's a menu
                    {
                        System.Diagnostics.Debug.WriteLine($"Adding menu to cart: {item.Id}");
                        _cartService.AddMenu(item.Id, 1);
                    }

                    // Show success message on UI thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _dialogService.ShowMessage($"{item.Name} added to cart", "Item Added",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    });

                    System.Diagnostics.Debug.WriteLine("Item added successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in AddToCart: {ex.Message}\n{ex.StackTrace}");

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ErrorMessage = $"Error adding to cart: {ex.Message}";
                        _dialogService.ShowMessage(ErrorMessage, "Error",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    });
                }
            });
        }

        private void ShowItemDetails(MenuItemViewModel item)
        {
            if (item == null)
                return;

            try
            {
                var viewModel = new ItemDetailsViewModel(
                    item,
                    _cartService,
                    _userSessionService,
                    _dialogService,
                    _categoryService);

                var dialog = new RestaurantApp.UI.Views.Dialogs.ItemDetailsDialog
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error showing item details: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
        public string CategoryName { get; set; } // Make sure this property exists
        public List<MenuDishViewModel> MenuItems { get; set; } = new List<MenuDishViewModel>();
    }

    public class DishItemViewModel : MenuItemViewModel
    {
        public DishItemViewModel()
        {
            IsMenu = false; // This is a dish, not a menu
        }

        public BitmapImage ImageSource { get; set; }
    }

    public class MenuDishViewModel
    {
        public int DishId { get; set; }
        public string DishName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "g"; // Unitatea de măsură, probabil grame
    }
}