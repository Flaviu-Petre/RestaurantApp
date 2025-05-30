﻿using System.Windows.Controls;
using RestaurantApp.UI.ViewModels.Admin;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.Infrastructure;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantApp.UI.Views.Admin
{
    public partial class CategoriesView : UserControl
    {
        public CategoriesView()
        {
            InitializeComponent();

            // Get required services
            var categoryService = App.ServiceProvider.GetService<ICategoryService>();
            var dialogService = App.ServiceProvider.GetService<IDialogService>();
            var messageBus = App.ServiceProvider.GetService<IMessageBus>();

            // Create and set ViewModel
            if (categoryService != null && dialogService != null && messageBus != null)
            {
                DataContext = new CategoryManagementViewModel(categoryService, dialogService, messageBus);
            }
            else
            {
                MessageBox.Show("Failed to initialize CategoriesView: Required services not available.",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}