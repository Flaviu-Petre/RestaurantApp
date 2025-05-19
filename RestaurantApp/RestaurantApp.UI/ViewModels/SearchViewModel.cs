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
    public class SearchViewModel : ViewModelBase
    {
        private readonly IDishService _dishService;
        private readonly IMenuService _menuService;
        private readonly IAllergenService _allergenService;
        private readonly IUserSessionService _userSessionService;
        private readonly ICartService _cartService;
        private readonly IDialogService _dialogService;

        public SearchViewModel(
            IDishService dishService,
            IMenuService menuService,
            IAllergenService allergenService,
            IUserSessionService userSessionService,
            ICartService cartService,
            IDialogService dialogService)
        {
            _dishService = dishService;
            _menuService = menuService;
            _allergenService = allergenService;
            _userSessionService = userSessionService;
            _cartService = cartService;
            _dialogService = dialogService;

            // Initialize search types
            SearchTypes = new List<string> { "By Name", "By Allergen" };
            SelectedSearchType = SearchTypes.First();
            IncludeAllergen = true;

            // Initialize commands
            SearchCommand = new AsyncRelayCommand(SearchAsync);
            ApplyAllergenFilterCommand = new AsyncRelayCommand(ApplyAllergenFilterAsync);
            ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);
            AddToCartCommand = new RelayCommand<MenuItemViewModel>(AddToCart, item => item != null);

            // Load allergens and initialize selected allergen
            LoadAllergensAsync().ConfigureAwait(false);
        }

        #region Properties

        private List<string> _searchTypes;
        public List<string> SearchTypes
        {
            get => _searchTypes;
            set => SetProperty(ref _searchTypes, value);
        }

        private string _selectedSearchType;
        public string SelectedSearchType
        {
            get => _selectedSearchType;
            set => SetProperty(ref _selectedSearchType, value);
        }

        private string _searchTerm;
        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        private ObservableCollection<AllergenViewModel> _allergens;
        public ObservableCollection<AllergenViewModel> Allergens
        {
            get => _allergens;
            set => SetProperty(ref _allergens, value);
        }

        private AllergenViewModel _selectedAllergen;
        public AllergenViewModel SelectedAllergen
        {
            get => _selectedAllergen;
            set => SetProperty(ref _selectedAllergen, value);
        }

        private bool _includeAllergen;
        public bool IncludeAllergen
        {
            get => _includeAllergen;
            set => SetProperty(ref _includeAllergen, value);
        }

        private ObservableCollection<MenuItemViewModel> _searchResults;
        public ObservableCollection<MenuItemViewModel> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        public bool HasResults => SearchResults != null && SearchResults.Any();

        public bool IsCustomer => _userSessionService.IsCustomer;

        #endregion

        #region Commands

        public ICommand SearchCommand { get; }
        public ICommand ApplyAllergenFilterCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand AddToCartCommand { get; }

        #endregion

        #region Methods

        private async Task LoadAllergensAsync()
        {
            try
            {
                IsBusy = true;
                var allergens = await _allergenService.GetAllAllergensAsync();

                var allergenViewModels = allergens.Select(a => new AllergenViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description
                }).ToList();

                Allergens = new ObservableCollection<AllergenViewModel>(allergenViewModels);

                if (Allergens.Any())
                {
                    SelectedAllergen = Allergens.First();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading allergens: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SearchAsync()
        {
            try
            {
                IsBusy = true;

                if (string.IsNullOrWhiteSpace(SearchTerm))
                {
                    SearchResults = new ObservableCollection<MenuItemViewModel>();
                    OnPropertyChanged(nameof(HasResults));
                    return;
                }

                if (SelectedSearchType == "By Name")
                {
                    // Search dishes by name
                    var dishes = await _dishService.SearchDishesByNameAsync(SearchTerm);
                    var dishViewModels = CreateDishViewModels(dishes);

                    // Search menus by name
                    var menus = await _menuService.SearchMenusByNameAsync(SearchTerm);
                    var menuViewModels = CreateMenuViewModels(menus);

                    // Combine results
                    var results = new List<MenuItemViewModel>();
                    results.AddRange(dishViewModels);
                    results.AddRange(menuViewModels);

                    SearchResults = new ObservableCollection<MenuItemViewModel>(results);
                }
                else
                {
                    // Allergen search will be handled by ApplyAllergenFilterAsync
                    await ApplyAllergenFilterAsync();
                }

                OnPropertyChanged(nameof(HasResults));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error performing search: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ApplyAllergenFilterAsync()
        {
            try
            {
                IsBusy = true;

                if (SelectedAllergen == null)
                {
                    SearchResults = new ObservableCollection<MenuItemViewModel>();
                    OnPropertyChanged(nameof(HasResults));
                    return;
                }

                // Search dishes by allergen
                var dishes = await _dishService.SearchDishesByAllergenAsync(SelectedAllergen.Id, IncludeAllergen);
                var dishViewModels = CreateDishViewModels(dishes);

                // Search menus by allergen
                var menus = await _menuService.SearchMenusByAllergenAsync(SelectedAllergen.Id, IncludeAllergen);
                var menuViewModels = CreateMenuViewModels(menus);

                // Combine results
                var results = new List<MenuItemViewModel>();
                results.AddRange(dishViewModels);
                results.AddRange(menuViewModels);

                SearchResults = new ObservableCollection<MenuItemViewModel>(results);
                OnPropertyChanged(nameof(HasResults));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error applying allergen filter: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ClearFiltersAsync()
        {
            SearchTerm = string.Empty;
            SelectedSearchType = SearchTypes.First();
            IncludeAllergen = true;

            if (Allergens != null && Allergens.Any())
            {
                SelectedAllergen = Allergens.First();
            }

            SearchResults = new ObservableCollection<MenuItemViewModel>();
            OnPropertyChanged(nameof(HasResults));

            await Task.CompletedTask;
        }

        private List<MenuItemViewModel> CreateDishViewModels(IEnumerable<Dish> dishes)
        {
            return dishes.Select(d => new DishItemViewModel
            {
                Id = d.Id,
                Name = d.Name,
                Description = "Delicious dish", // Default description 
                Price = d.Price,
                PortionQuantity = d.PortionQuantity,
                CategoryId = d.CategoryId,
                ImageUrl = GetDishImageUrl(d),
                AllergensList = GetDishAllergensList(d),
                HasAllergens = (d.DishAllergens?.Any() ?? false),
                IsAvailable = d.TotalQuantity > 0
            }).ToList<MenuItemViewModel>();
        }

        private List<MenuItemViewModel> CreateMenuViewModels(IEnumerable<Menu> menus)
        {
            return menus.Select(m => new MenuItemViewModel
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description ?? "Delicious menu", // Default if null

                // Show price WITHOUT discount - just sum the dish prices
                Price = CalculateMenuPriceWithoutDiscount(m),

                // Calculate and show the correct portion quantity
                PortionQuantity = CalculateMenuPortionQuantity(m),

                CategoryId = m.CategoryId,
                ImageUrl = "/Images/default-menu.png", // Default menu image
                AllergensList = GetMenuAllergensList(m),
                HasAllergens = DoesMenuHaveAllergens(m),
                IsAvailable = IsMenuAvailable(m),
                IsMenu = true // Indicate this is a menu, not a dish
            }).ToList<MenuItemViewModel>();
        }

        // New method to calculate menu price WITHOUT applying discount
        private decimal CalculateMenuPriceWithoutDiscount(Menu menu)
        {
            // If menu dishes are loaded, sum up their prices
            if (menu.MenuDishes != null && menu.MenuDishes.Any())
            {
                return menu.MenuDishes.Sum(md => md.Dish?.Price ?? 0);
            }

            // If dishes aren't loaded, try to load and calculate
            try
            {
                // Load menu with dishes
                var menuTask = Task.Run(async () => await _menuService.GetMenuWithDetailsAsync(menu.Id));
                menuTask.Wait();
                var menuWithDishes = menuTask.Result;

                if (menuWithDishes?.MenuDishes != null && menuWithDishes.MenuDishes.Any())
                {
                    // Calculate the sum of dish prices without applying discount
                    return menuWithDishes.MenuDishes.Sum(md => md.Dish?.Price ?? 0);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating non-discounted menu price: {ex.Message}");
            }

            // Default to zero if calculation fails
            return 0;
        }

        // New method to calculate menu portion quantity
        private decimal CalculateMenuPortionQuantity(Menu menu)
        {
            decimal totalPortionQuantity = 0;

            // If menu dishes are loaded, sum up portion quantities
            if (menu.MenuDishes != null && menu.MenuDishes.Any())
            {
                foreach (var menuDish in menu.MenuDishes)
                {
                    if (menuDish.Dish != null)
                    {
                        // Add each dish's portion quantity
                        totalPortionQuantity += menuDish.Dish.PortionQuantity;
                    }
                }
            }
            else
            {
                // If dishes aren't loaded, try to load and calculate
                try
                {
                    // Load menu with dishes
                    var menuTask = Task.Run(async () => await _menuService.GetMenuWithDetailsAsync(menu.Id));
                    menuTask.Wait();
                    var menuWithDishes = menuTask.Result;

                    if (menuWithDishes?.MenuDishes != null && menuWithDishes.MenuDishes.Any())
                    {
                        // Calculate the sum of dish portion quantities
                        foreach (var menuDish in menuWithDishes.MenuDishes)
                        {
                            if (menuDish.Dish != null)
                            {
                                totalPortionQuantity += menuDish.Dish.PortionQuantity;
                            }
                        }
                    }
                    else
                    {
                        // If loading fails, use a reasonable default
                        totalPortionQuantity = 200;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calculating menu portion: {ex.Message}");
                    // Use a default value
                    totalPortionQuantity = 200;
                }
            }

            // Round to a whole number
            return Math.Round(totalPortionQuantity);
        }

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

        // Fix the AddToCart method in SearchViewModel.cs
        private void AddToCart(MenuItemViewModel item)
        {
            // Run the operation on a background thread to avoid UI freezing
            Task.Run(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"SearchViewModel.AddToCart called for item: {item?.Name}");

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

        #endregion
    }

    // Helper view model classes
    public class AllergenViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}