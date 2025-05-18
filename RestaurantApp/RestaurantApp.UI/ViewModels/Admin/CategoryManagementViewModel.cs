// This should be in a new file: ViewModels/Admin/CategoryManagementViewModel.cs
using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantApp.UI.ViewModels.Admin
{
    public class CategoryManagementViewModel : ViewModelBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IDialogService _dialogService;
        private readonly IMessageBus _messageBus;

        public CategoryManagementViewModel(
            ICategoryService categoryService,
            IDialogService dialogService,
            IMessageBus messageBus)
        {
            _categoryService = categoryService;
            _dialogService = dialogService;
            _messageBus = messageBus;

            // Initialize commands
            AddNewCategoryCommand = new RelayCommand(AddNewCategory);
            SaveCategoryCommand = new AsyncRelayCommand(SaveCategoryAsync, CanSaveCategory);
            DeleteCategoryCommand = new AsyncRelayCommand(DeleteCategoryAsync, () => SelectedCategory != null);

            // Load data
            LoadCategoriesAsync().ConfigureAwait(false);
        }

        #region Properties

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
                    // Update properties based on selected category
                    CategoryName = _selectedCategory?.Name ?? string.Empty;
                    CategoryDescription = _selectedCategory?.Description ?? string.Empty;
                    OnPropertyChanged(nameof(HasSelectedCategory));
                }
            }
        }

        private string _categoryName;
        public string CategoryName
        {
            get => _categoryName;
            set => SetProperty(ref _categoryName, value);
        }

        private string _categoryDescription;
        public string CategoryDescription
        {
            get => _categoryDescription;
            set => SetProperty(ref _categoryDescription, value);
        }

        public bool HasSelectedCategory => SelectedCategory != null;

        #endregion

        #region Commands

        public ICommand AddNewCategoryCommand { get; }
        public ICommand SaveCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }

        #endregion

        #region Methods

        private async Task LoadCategoriesAsync()
        {
            try
            {
                IsBusy = true;
                var categories = await _categoryService.GetAllCategoriesAsync();
                Categories = new ObservableCollection<Category>(categories);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Error loading categories: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AddNewCategory()
        {
            // Create a new category and select it
            SelectedCategory = new Category();
            CategoryName = string.Empty;
            CategoryDescription = string.Empty;
        }

        private async Task SaveCategoryAsync()
        {
            try
            {
                IsBusy = true;

                // Update category properties
                bool isNewCategory = SelectedCategory.Id == 0;
                SelectedCategory.Name = CategoryName;
                SelectedCategory.Description = CategoryDescription;

                if (isNewCategory)
                {
                    // Create new category
                    var createdCategory = await _categoryService.CreateCategoryAsync(SelectedCategory);
                    Categories.Add(createdCategory);
                    SelectedCategory = createdCategory;
                }
                else
                {
                    // Update existing category
                    await _categoryService.UpdateCategoryAsync(SelectedCategory);

                    // Refresh the collection to update the UI
                    int index = Categories.IndexOf(Categories.FirstOrDefault(c => c.Id == SelectedCategory.Id));
                    if (index >= 0)
                    {
                        Categories[index] = SelectedCategory;
                    }
                }

                _dialogService.ShowMessage("Category saved successfully.", "Success",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                // Notify that data has changed
                _messageBus.Publish(new RefreshDataMessage());
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Error saving category: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteCategoryAsync()
        {
            if (SelectedCategory == null)
                return;

            var result = _dialogService.ShowMessage("Are you sure you want to delete this category?",
                "Confirm Delete", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    await _categoryService.DeleteCategoryAsync(SelectedCategory.Id);

                    // Remove from collection
                    Categories.Remove(SelectedCategory);
                    SelectedCategory = null;

                    // Notify that data has changed
                    _messageBus.Publish(new RefreshDataMessage());

                    _dialogService.ShowMessage("Category deleted successfully.", "Success",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessage($"Error deleting category: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private bool CanSaveCategory()
        {
            return !string.IsNullOrWhiteSpace(CategoryName);
        }

        #endregion
    }
}