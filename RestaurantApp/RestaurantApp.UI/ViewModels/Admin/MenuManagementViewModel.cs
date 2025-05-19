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

namespace RestaurantApp.UI.ViewModels.Admin
{
    public class MenuManagementViewModel : ViewModelBase
    {
        private readonly IMenuService _menuService;
        private readonly ICategoryService _categoryService;
        private readonly IDishService _dishService;
        private readonly IConfigurationService _configService;
        private readonly IDialogService _dialogService;
        private readonly IMessageBus _messageBus;
        private decimal _menuDiscountPercentage;

        public MenuManagementViewModel(
            IMenuService menuService,
            ICategoryService categoryService,
            IDishService dishService,
            IConfigurationService configService,
            IDialogService dialogService,
            IMessageBus messageBus)
        {
            _menuService = menuService;
            _categoryService = categoryService;
            _dishService = dishService;
            _configService = configService;
            _dialogService = dialogService;
            _messageBus = messageBus;

            // Initialize commands
            AddNewMenuCommand = new RelayCommand(AddNewMenu);
            SaveMenuCommand = new AsyncRelayCommand(SaveMenuAsync, CanSaveMenu);
            DeleteMenuCommand = new AsyncRelayCommand(DeleteMenuAsync, () => SelectedMenu != null);
            AddDishToMenuCommand = new RelayCommand(AddDishToMenu, CanAddDishToMenu);
            RemoveDishCommand = new RelayCommand<MenuDishViewModel>(RemoveDishFromMenu);

            // Initialize collections
            MenuDishes = new ObservableCollection<MenuDishViewModel>();
            Menus = new ObservableCollection<Menu>();
            FilteredMenus = new ObservableCollection<Menu>();
            Categories = new ObservableCollection<Category>();
            AvailableDishes = new ObservableCollection<Dish>();

            // Initialize properties
            NewDishQuantity = 100; // Default quantity

            // Load data
            Task.Run(() => LoadInitialDataAsync());
        }

        #region Properties

        private ObservableCollection<Menu> _menus;
        public ObservableCollection<Menu> Menus
        {
            get => _menus;
            set => SetProperty(ref _menus, value);
        }

        private ObservableCollection<Menu> _filteredMenus;
        public ObservableCollection<Menu> FilteredMenus
        {
            get => _filteredMenus;
            set => SetProperty(ref _filteredMenus, value);
        }

        private Menu _selectedMenu;
        public Menu SelectedMenu
        {
            get => _selectedMenu;
            set
            {
                if (SetProperty(ref _selectedMenu, value))
                {
                    // Update properties based on selected menu
                    MenuName = _selectedMenu?.Name ?? string.Empty;
                    MenuDescription = _selectedMenu?.Description ?? string.Empty;
                    SelectedCategory = Categories?.FirstOrDefault(c => c.Id == _selectedMenu?.CategoryId);
                    RefreshSelectedMenuDishes();
                    CalculateMenuPrice();
                    OnPropertyChanged(nameof(HasSelectedMenu));
                }
            }
        }

        private string _menuName;
        public string MenuName
        {
            get => _menuName;
            set
            {
                if (SetProperty(ref _menuName, value))
                {
                    (SaveMenuCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _menuDescription;
        public string MenuDescription
        {
            get => _menuDescription;
            set => SetProperty(ref _menuDescription, value);
        }

        private decimal _menuPrice;
        public decimal MenuPrice
        {
            get => _menuPrice;
            set => SetProperty(ref _menuPrice, value);
        }

        private ObservableCollection<Category> _categories;
        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    (SaveMenuCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private Category _selectedFilterCategory;
        public Category SelectedFilterCategory
        {
            get => _selectedFilterCategory;
            set
            {
                if (SetProperty(ref _selectedFilterCategory, value))
                {
                    ApplyFilters();
                }
            }
        }

        private string _searchTerm;
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (SetProperty(ref _searchTerm, value))
                {
                    ApplyFilters();
                }
            }
        }

        private ObservableCollection<MenuDishViewModel> _menuDishes;
        public ObservableCollection<MenuDishViewModel> MenuDishes
        {
            get => _menuDishes;
            set => SetProperty(ref _menuDishes, value);
        }

        private ObservableCollection<Dish> _availableDishes;
        public ObservableCollection<Dish> AvailableDishes
        {
            get => _availableDishes;
            set => SetProperty(ref _availableDishes, value);
        }

        private Dish _selectedDish;
        public Dish SelectedDish
        {
            get => _selectedDish;
            set
            {
                if (SetProperty(ref _selectedDish, value))
                {
                    (AddDishToMenuCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private decimal _newDishQuantity;
        public decimal NewDishQuantity
        {
            get => _newDishQuantity;
            set => SetProperty(ref _newDishQuantity, value);
        }

        public bool HasSelectedMenu => SelectedMenu != null;

        #endregion

        #region Commands

        public ICommand AddNewMenuCommand { get; }
        public ICommand SaveMenuCommand { get; }
        public ICommand DeleteMenuCommand { get; }
        public ICommand AddDishToMenuCommand { get; }
        public ICommand RemoveDishCommand { get; }

        #endregion

        #region Methods

        private async Task LoadInitialDataAsync()
        {
            try
            {
                IsBusy = true;

                // Load menu discount percentage from configuration
                _menuDiscountPercentage = await _configService.GetMenuDiscountPercentageAsync();

                // Load categories
                var categoriesData = await _categoryService.GetAllCategoriesAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Add "All Categories" for filtering
                    var categoriesList = new List<Category>(categoriesData);
                    categoriesList.Insert(0, new Category { Id = 0, Name = "All Categories" });

                    Categories = new ObservableCollection<Category>(categoriesList);
                    SelectedFilterCategory = Categories.FirstOrDefault(); // Select "All Categories"
                });

                // Load dishes for the available dishes dropdown
                var availableDishesData = await _dishService.GetAllDishesAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AvailableDishes = new ObservableCollection<Dish>(availableDishesData
                        .Where(d => d.TotalQuantity > 0) // Only show available dishes
                        .OrderBy(d => d.Name));

                    if (AvailableDishes.Any())
                    {
                        SelectedDish = AvailableDishes.First();
                    }
                });

                // Load menus
                await LoadMenusAsync();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ErrorMessage = $"Error loading data: {ex.Message}";
                    _dialogService.ShowMessage(ErrorMessage, "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadMenusAsync()
        {
            try
            {
                var menusData = await _menuService.GetAllMenusAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Menus = new ObservableCollection<Menu>(menusData);
                    ApplyFilters();
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ErrorMessage = $"Error loading menus: {ex.Message}";
                    _dialogService.ShowMessage(ErrorMessage, "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                });
            }
        }

        private void ApplyFilters()
        {
            if (Menus == null)
                return;

            IEnumerable<Menu> filtered = Menus;

            // Apply category filter
            if (SelectedFilterCategory != null && SelectedFilterCategory.Id > 0)
            {
                filtered = filtered.Where(m => m.CategoryId == SelectedFilterCategory.Id);
            }

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filtered = filtered.Where(m => m.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                             m.Description?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true);
            }

            FilteredMenus = new ObservableCollection<Menu>(filtered);
        }

        private async Task RefreshSelectedMenuDishes()
        {
            if (SelectedMenu == null)
            {
                MenuDishes.Clear();
                return;
            }

            try
            {
                // Get menu with detailed dishes
                var menuWithDishes = await _menuService.GetMenuWithDetailsAsync(SelectedMenu.Id);

                if (menuWithDishes != null && menuWithDishes.MenuDishes != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var menuDishViewModels = menuWithDishes.MenuDishes.Select(md => new MenuDishViewModel
                        {
                            DishId = md.DishId,
                            DishName = md.Dish?.Name ?? "Unknown Dish",
                            DishPrice = md.Dish?.Price ?? 0,
                            QuantityInMenu = md.QuantityInMenu
                        }).ToList();

                        MenuDishes = new ObservableCollection<MenuDishViewModel>(menuDishViewModels);
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MenuDishes.Clear();
                    });
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ErrorMessage = $"Error loading menu dishes: {ex.Message}";
                    MenuDishes.Clear();
                });
            }
        }

        private void AddNewMenu()
        {
            // Create a new menu with default values
            SelectedMenu = new Menu
            {
                Name = "New Menu",
                Description = "Menu description",
                CategoryId = Categories?.FirstOrDefault(c => c.Id > 0)?.Id ?? 0,
                MenuDishes = new List<MenuDish>()
            };

            MenuName = SelectedMenu.Name;
            MenuDescription = SelectedMenu.Description;
            SelectedCategory = Categories?.FirstOrDefault(c => c.Id == SelectedMenu.CategoryId);
            MenuDishes.Clear();
            CalculateMenuPrice();
        }

        private async Task SaveMenuAsync()
        {
            try
            {
                IsBusy = true;

                // Update menu properties
                bool isNewMenu = SelectedMenu.Id == 0;
                SelectedMenu.Name = MenuName;
                SelectedMenu.Description = MenuDescription;
                SelectedMenu.CategoryId = SelectedCategory?.Id ?? 0;

                // Prepare menu dishes
                var menuDishes = MenuDishes.Select(md => new MenuDish
                {
                    DishId = md.DishId,
                    QuantityInMenu = md.QuantityInMenu
                }).ToList();

                if (isNewMenu)
                {
                    // Create new menu
                    var createdMenu = await _menuService.CreateMenuAsync(SelectedMenu, menuDishes);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Menus.Add(createdMenu);
                        SelectedMenu = createdMenu;
                        ApplyFilters();
                    });
                }
                else
                {
                    // Update existing menu
                    await _menuService.UpdateMenuAsync(SelectedMenu, menuDishes);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Update the menu in the collections
                        int index = Menus.IndexOf(Menus.FirstOrDefault(m => m.Id == SelectedMenu.Id));
                        if (index >= 0)
                        {
                            Menus[index] = SelectedMenu;
                        }

                        ApplyFilters();
                    });
                }

                // Refresh the selected menu's dishes
                await RefreshSelectedMenuDishes();
                CalculateMenuPrice();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _dialogService.ShowMessage("Menu saved successfully.", "Success",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                });

                // Notify that data has changed
                _messageBus.Publish(new RefreshDataMessage());
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ErrorMessage = $"Error saving menu: {ex.Message}";
                    _dialogService.ShowMessage(ErrorMessage, "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteMenuAsync()
        {
            if (SelectedMenu == null)
                return;

            var result = _dialogService.ShowMessage("Are you sure you want to delete this menu?",
                "Confirm Delete", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    await _menuService.DeleteMenuAsync(SelectedMenu.Id);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Remove from collections
                        Menus.Remove(SelectedMenu);
                        FilteredMenus.Remove(SelectedMenu);
                        SelectedMenu = null;
                    });

                    // Notify that data has changed
                    _messageBus.Publish(new RefreshDataMessage());

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _dialogService.ShowMessage("Menu deleted successfully.", "Success",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ErrorMessage = $"Error deleting menu: {ex.Message}";
                        _dialogService.ShowMessage(ErrorMessage, "Error",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    });
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private void AddDishToMenu()
        {
            if (SelectedDish == null || NewDishQuantity <= 0)
                return;

            // Check if dish already exists in the menu
            var existingDish = MenuDishes.FirstOrDefault(md => md.DishId == SelectedDish.Id);

            if (existingDish != null)
            {
                // Update quantity
                existingDish.QuantityInMenu = NewDishQuantity;
            }
            else
            {
                // Add new dish
                MenuDishes.Add(new MenuDishViewModel
                {
                    DishId = SelectedDish.Id,
                    DishName = SelectedDish.Name,
                    DishPrice = SelectedDish.Price,
                    QuantityInMenu = NewDishQuantity
                });
            }

            // Reset quantity to default
            NewDishQuantity = 100;

            // Recalculate menu price
            CalculateMenuPrice();
        }

        private void RemoveDishFromMenu(MenuDishViewModel menuDish)
        {
            if (menuDish == null)
                return;

            MenuDishes.Remove(menuDish);
            CalculateMenuPrice();
        }

        private void CalculateMenuPrice()
        {
            decimal totalPrice = 0;

            foreach (var menuDish in MenuDishes)
            {
                totalPrice += menuDish.DishPrice;
            }

            // Apply discount
            decimal discountMultiplier = 1 - (_menuDiscountPercentage / 100);
            MenuPrice = totalPrice * discountMultiplier;
        }

        private bool CanSaveMenu()
        {
            return !string.IsNullOrWhiteSpace(MenuName) &&
                   SelectedCategory != null &&
                   SelectedCategory.Id > 0 &&
                   MenuDishes.Count > 0;
        }

        private bool CanAddDishToMenu()
        {
            return SelectedDish != null && NewDishQuantity > 0;
        }

        #endregion
    }

    public class MenuDishViewModel : ViewModelBase
    {
        private int _dishId;
        public int DishId
        {
            get => _dishId;
            set => SetProperty(ref _dishId, value);
        }

        private string _dishName;
        public string DishName
        {
            get => _dishName;
            set => SetProperty(ref _dishName, value);
        }

        private decimal _dishPrice;
        public decimal DishPrice
        {
            get => _dishPrice;
            set => SetProperty(ref _dishPrice, value);
        }

        private decimal _quantityInMenu;
        public decimal QuantityInMenu
        {
            get => _quantityInMenu;
            set => SetProperty(ref _quantityInMenu, value);
        }
    }
}