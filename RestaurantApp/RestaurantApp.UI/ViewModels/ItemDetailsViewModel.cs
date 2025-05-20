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
    public class ItemDetailsViewModel : ViewModelBase
    {
        private readonly ICartService _cartService;
        private readonly IUserSessionService _userSessionService;
        private readonly IDialogService _dialogService;
        private readonly ICategoryService _categoryService;

        public ItemDetailsViewModel(
            MenuItemViewModel item,
            ICartService cartService,
            IUserSessionService userSessionService,
            IDialogService dialogService,
            ICategoryService categoryService) // Adăugăm serviciul de categorii
        {
            _cartService = cartService;
            _userSessionService = userSessionService;
            _dialogService = dialogService;
            _categoryService = categoryService;

            // Copy properties from the menu item
            Id = item.Id;
            Name = item.Name;
            Description = item.Description;
            HasDescription = !string.IsNullOrWhiteSpace(item.Description);
            Price = item.Price;
            PortionQuantity = item.PortionQuantity;
            CategoryId = item.CategoryId;
            ImageUrl = item.ImageUrl;
            AllergensList = item.AllergensList; 
            HasAllergens = !string.IsNullOrWhiteSpace(item.AllergensList);
            IsAvailable = item.IsAvailable;
            IsMenu = item.IsMenu;

            // Set additional properties
            CanAddToCart = _userSessionService.IsCustomer;

            // Initialize commands
            AddToCartCommand = new RelayCommand(AddToCart, () => CanAddToCart && IsAvailable);

            // Load menu items if this is a menu
            if (IsMenu)
            {
                MenuItems = new ObservableCollection<MenuItemDetail>();
                // Aici poți popula elementele meniului dacă ai logica necesară
            }

            // Încărcăm asincron numele categoriei
            LoadCategoryNameAsync(CategoryId);
        }

        // Properties
        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public bool HasDescription { get; }
        public decimal Price { get; }
        public decimal PortionQuantity { get; }
        public int CategoryId { get; }

        private string _categoryName = "Loading...";
        public string CategoryName
        {
            get => _categoryName;
            private set => SetProperty(ref _categoryName, value);
        }

        public string ImageUrl { get; }
        public string AllergensList { get; }
        public bool HasAllergens { get; }
        public bool IsAvailable { get; }
        public bool IsMenu { get; }
        public bool CanAddToCart { get; }
        public ObservableCollection<MenuItemDetail> MenuItems { get; private set; } = new ObservableCollection<MenuItemDetail>();

        // Commands
        public ICommand AddToCartCommand { get; }

        // Method to load category name
        private async void LoadCategoryNameAsync(int categoryId)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                if (category != null)
                {
                    CategoryName = category.Name;
                }
                else
                {
                    CategoryName = "Unknown Category";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading category: {ex.Message}");
                CategoryName = "Unknown Category";
            }
        }

        // Methods
        private void AddToCart()
        {
            try
            {
                if (!_userSessionService.IsLoggedIn || !_userSessionService.IsCustomer)
                {
                    _dialogService.ShowMessage("You need to be logged in as a customer to add items to cart",
                        "Login Required", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    return;
                }

                if (!IsAvailable)
                {
                    _dialogService.ShowMessage("This item is currently not available",
                        "Unavailable", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // Add the item based on type
                if (!IsMenu) // It's a dish
                {
                    _cartService.AddDish(Id, 1);
                }
                else // It's a menu
                {
                    _cartService.AddMenu(Id, 1);
                }

                // Show success message
                _dialogService.ShowMessage($"{Name} added to cart", "Item Added",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding to cart: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }

    public class MenuItemDetail
    {
        public string Name { get; set; }
        public decimal Quantity { get; set; }
    }
}