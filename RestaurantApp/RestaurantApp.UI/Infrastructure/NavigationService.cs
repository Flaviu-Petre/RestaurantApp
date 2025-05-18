using Microsoft.Extensions.DependencyInjection;
using RestaurantApp.Core.Services.Interfaces;
using RestaurantApp.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace RestaurantApp.UI.Infrastructure
{
    public interface INavigationService
    {
        void NavigateTo(string viewName);
        void NavigateTo(string viewName, object parameter);
        void GoBack();
        bool CanGoBack { get; }

    }

    public class NavigationService : INavigationService
    {
        private readonly Dictionary<string, Type> _viewsMap = new Dictionary<string, Type>();
        private readonly ContentControl _contentControl;
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(ContentControl contentControl, IServiceProvider serviceProvider = null)
        {
            _contentControl = contentControl ?? throw new ArgumentNullException(nameof(contentControl));
            _serviceProvider = serviceProvider;
        }

        public void RegisterView(string viewName, Type viewType)
        {
            if (string.IsNullOrWhiteSpace(viewName))
                throw new ArgumentException("View name cannot be null or empty", nameof(viewName));

            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            if (!typeof(UserControl).IsAssignableFrom(viewType))
                throw new ArgumentException("View type must derive from UserControl", nameof(viewType));

            _viewsMap[viewName] = viewType;
        }

        public void NavigateTo(string viewName)
        {
            NavigateTo(viewName, null);
        }

        public void NavigateTo(string viewName, object parameter)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"NavigateTo called with viewName: {viewName}");

                if (!_viewsMap.TryGetValue(viewName, out Type viewType))
                {
                    var registeredViews = string.Join(", ", _viewsMap.Keys);
                    System.Diagnostics.Debug.WriteLine($"View '{viewName}' not found. Registered views: {registeredViews}");
                    throw new ArgumentException($"View '{viewName}' is not registered", nameof(viewName));
                }

                // Create the view
                var view = (UserControl)Activator.CreateInstance(viewType);
                System.Diagnostics.Debug.WriteLine($"Created view of type: {viewType.Name}");

                // Set the view as content - just create the view without trying to set up its ViewModel
                _contentControl.Content = view;
                System.Diagnostics.Debug.WriteLine($"Set content to view: {viewType.Name}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                throw;
            }
        }

        public void GoBack()
        {
            // Implementation for GoBack if needed
        }

        public bool CanGoBack => false; // Implement as needed
    }
}