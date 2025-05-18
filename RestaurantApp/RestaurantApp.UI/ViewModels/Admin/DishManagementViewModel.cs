using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantApp.UI.ViewModels.Admin
{
    public class DishManagementViewModel : ViewModelBase
    {
        private readonly IDishService _dishService;
        private readonly ICategoryService _categoryService;
        private readonly IAllergenService _allergenService;
        private readonly IDialogService _dialogService;
        private readonly IMessageBus _messageBus;

        public DishManagementViewModel(
            IDishService dishService,
            ICategoryService categoryService,
            IAllergenService allergenService,
            IDialogService dialogService,
            IMessageBus messageBus)
        {
            _dishService = dishService;
            _categoryService = categoryService;
            _allergenService = allergenService;
            _dialogService = dialogService;
            _messageBus = messageBus;

            // Initialize commands
            AddNewDishCommand = new RelayCommand(AddNewDish);
            SaveDishCommand = new AsyncRelayCommand(SaveDishAsync, CanSaveDish);
            DeleteDishCommand = new AsyncRelayCommand(DeleteDishAsync, () => SelectedDish != null);
            AddImageCommand = new AsyncRelayCommand(AddImageAsync);
            RemoveImageCommand = new RelayCommand<DishImage>(RemoveImage);

            // Load initial data
            LoadDataAsync().ConfigureAwait(false);
        }

        #region Properties

        private ObservableCollection<Dish> _dishes;
        public ObservableCollection<Dish> Dishes
        {
            get => _dishes;
            set => SetProperty(ref _dishes, value);
        }

        private ObservableCollection<Dish> _filteredDishes;
        public ObservableCollection<Dish> FilteredDishes
        {
            get => _filteredDishes;
            set => SetProperty(ref _filteredDishes, value);
        }

        private Dish _selectedDish;
        public Dish SelectedDish
        {
            get => _selectedDish;
            set
            {
                if (SetProperty(ref _selectedDish, value))
                {
                    // Update properties based on selected dish
                    DishName = _selectedDish?.Name ?? string.Empty;
                    DishPrice = _selectedDish?.Price ?? 0;
                    DishPortionQuantity = _selectedDish?.PortionQuantity ?? 0;
                    DishTotalQuantity = _selectedDish?.TotalQuantity ?? 0;
                    SelectedCategory = Categories?.FirstOrDefault(c => c.Id == _selectedDish?.CategoryId);
                    DishImages = _selectedDish?.Images != null
                        ? new ObservableCollection<DishImage>(_selectedDish.Images)
                        : new ObservableCollection<DishImage>();

                    // Update allergens selection
                    UpdateSelectedAllergens();

                    OnPropertyChanged(nameof(HasSelectedDish));
                }
            }
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
            set => SetProperty(ref _selectedCategory, value);
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

        private decimal _dishPortionQuantity;
        public decimal DishPortionQuantity
        {
            get => _dishPortionQuantity;
            set => SetProperty(ref _dishPortionQuantity, value);
        }

        private decimal _dishTotalQuantity;
        public decimal DishTotalQuantity
        {
            get => _dishTotalQuantity;
            set => SetProperty(ref _dishTotalQuantity, value);
        }

        private ObservableCollection<DishImage> _dishImages;
        public ObservableCollection<DishImage> DishImages
        {
            get => _dishImages;
            set => SetProperty(ref _dishImages, value);
        }

        private ObservableCollection<AllergenViewModel> _allergens;
        public ObservableCollection<AllergenViewModel> Allergens
        {
            get => _allergens;
            set => SetProperty(ref _allergens, value);
        }

        public bool HasSelectedDish => SelectedDish != null;

        #endregion

        #region Commands

        public ICommand AddNewDishCommand { get; }
        public ICommand SaveDishCommand { get; }
        public ICommand DeleteDishCommand { get; }
        public ICommand AddImageCommand { get; }
        public ICommand RemoveImageCommand { get; }

        #endregion

        #region Methods

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;

                // Load categories
                var categories = await _categoryService.GetAllCategoriesAsync();
                Categories = new ObservableCollection<Category>(categories);

                // Add "All Categories" option for filtering
                var allCategories = new List<Category>(categories);
                allCategories.Insert(0, new Category { Id = 0, Name = "All Categories" });
                Categories = new ObservableCollection<Category>(allCategories);
                SelectedFilterCategory = Categories.FirstOrDefault();

                // Load dishes
                var dishes = await _dishService.GetAllDishesAsync();
                Dishes = new ObservableCollection<Dish>(dishes);
                FilteredDishes = new ObservableCollection<Dish>(dishes);

                // Load allergens
                var allergens = await _allergenService.GetAllAllergensAsync();
                var allergenViewModels = allergens.Select(a => new AllergenViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    IsSelected = false
                }).ToList();
                Allergens = new ObservableCollection<AllergenViewModel>(allergenViewModels);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading data: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AddNewDish()
        {
            // Create a new dish and select it
            SelectedDish = new Dish
            {
                CategoryId = Categories?.FirstOrDefault(c => c.Id > 0)?.Id ?? 0,
                Images = new List<DishImage>(),
                DishAllergens = new List<DishAllergen>()
            };

            DishName = string.Empty;
            DishPrice = 0;
            DishPortionQuantity = 0;
            DishTotalQuantity = 0;
            SelectedCategory = Categories?.FirstOrDefault(c => c.Id > 0);
            DishImages = new ObservableCollection<DishImage>();

            // Reset allergen selection
            if (Allergens != null)
            {
                foreach (var allergen in Allergens)
                {
                    allergen.IsSelected = false;
                }
            }
        }

        private async Task SaveDishAsync()
        {
            try
            {
                IsBusy = true;

                // Update dish properties
                bool isNewDish = SelectedDish.Id == 0;
                SelectedDish.Name = DishName;
                SelectedDish.Price = DishPrice;
                SelectedDish.PortionQuantity = DishPortionQuantity;
                SelectedDish.TotalQuantity = DishTotalQuantity;
                SelectedDish.CategoryId = SelectedCategory?.Id ?? 0;

                // Update images
                SelectedDish.Images = DishImages.ToList();

                // Update allergens
                UpdateDishAllergens();

                if (isNewDish)
                {
                    // Create new dish
                    var createdDish = await _dishService.CreateDishAsync(SelectedDish);
                    Dishes.Add(createdDish);
                    SelectedDish = createdDish;
                }
                else
                {
                    // Update existing dish
                    await _dishService.UpdateDishAsync(SelectedDish);

                    // Refresh the collection to update the UI
                    int index = Dishes.IndexOf(Dishes.FirstOrDefault(d => d.Id == SelectedDish.Id));
                    if (index >= 0)
                    {
                        Dishes[index] = SelectedDish;
                    }
                }

                // Refresh filtered dishes
                ApplyFilters();

                _dialogService.ShowMessage("Dish saved successfully.", "Success",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                // Notify that data has changed
                _messageBus.Publish(new RefreshDataMessage());
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving dish: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteDishAsync()
        {
            if (SelectedDish == null)
                return;

            var result = _dialogService.ShowMessage("Are you sure you want to delete this dish?",
                "Confirm Delete", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    await _dishService.DeleteDishAsync(SelectedDish.Id);

                    // Remove from collections
                    Dishes.Remove(SelectedDish);
                    FilteredDishes.Remove(SelectedDish);
                    SelectedDish = null;

                    // Notify that data has changed
                    _messageBus.Publish(new RefreshDataMessage());

                    _dialogService.ShowMessage("Dish deleted successfully.", "Success",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error deleting dish: {ex.Message}";
                    _dialogService.ShowMessage(ErrorMessage, "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task AddImageAsync()
        {
            if (SelectedDish == null)
                return;

            try
            {
                bool success = _dialogService.ShowOpenFileDialog("Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png", out string filePath);
                if (success && !string.IsNullOrEmpty(filePath))
                {
                    // Create a copy of the image in the application directory
                    string fileName = Path.GetFileName(filePath);
                    string destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Dishes");

                    // Create the directory if it doesn't exist
                    if (!Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    string destPath = Path.Combine(destDir, $"{Guid.NewGuid()}_{fileName}");
                    File.Copy(filePath, destPath, true);

                    // Create a new DishImage
                    var dishImage = new DishImage
                    {
                        ImagePath = destPath,
                        DishId = SelectedDish.Id
                    };

                    // Add to the list
                    DishImages.Add(dishImage);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding image: {ex.Message}";
                _dialogService.ShowMessage(ErrorMessage, "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            await Task.CompletedTask;
        }

        private void RemoveImage(DishImage image)
        {
            if (image == null)
                return;

            DishImages.Remove(image);
        }

        private void ApplyFilters()
        {
            if (Dishes == null)
                return;

            IEnumerable<Dish> filtered = Dishes;

            // Apply category filter
            if (SelectedFilterCategory != null && SelectedFilterCategory.Id > 0)
            {
                filtered = filtered.Where(d => d.CategoryId == SelectedFilterCategory.Id);
            }

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filtered = filtered.Where(d => d.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            FilteredDishes = new ObservableCollection<Dish>(filtered);
        }

        private void UpdateSelectedAllergens()
        {
            if (SelectedDish == null || Allergens == null)
                return;

            // Reset all allergens
            foreach (var allergen in Allergens)
            {
                allergen.IsSelected = false;
            }

            // Set selected allergens based on the dish
            if (SelectedDish.DishAllergens != null)
            {
                foreach (var dishAllergen in SelectedDish.DishAllergens)
                {
                    var allergen = Allergens.FirstOrDefault(a => a.Id == dishAllergen.AllergenId);
                    if (allergen != null)
                    {
                        allergen.IsSelected = true;
                    }
                }
            }
        }

        private void UpdateDishAllergens()
        {
            if (SelectedDish == null || Allergens == null)
                return;

            // Create new dish allergens collection
            var dishAllergens = new List<DishAllergen>();

            // Add selected allergens
            foreach (var allergen in Allergens.Where(a => a.IsSelected))
            {
                dishAllergens.Add(new DishAllergen
                {
                    DishId = SelectedDish.Id,
                    AllergenId = allergen.Id
                });
            }

            // Update the dish
            SelectedDish.DishAllergens = dishAllergens;
        }

        private bool CanSaveDish()
        {
            return !string.IsNullOrWhiteSpace(DishName) &&
                   DishPrice >= 0 &&
                   DishPortionQuantity > 0 &&
                   DishTotalQuantity >= 0 &&
                   SelectedCategory != null &&
                   SelectedCategory.Id > 0;
        }

        #endregion
    }

    public class AllergenViewModel : ViewModelBase
    {
        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}