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
        private readonly Stack<UserControl> _navigationStack = new Stack<UserControl>();
        private readonly Stack<object> _parametersStack = new Stack<object>();
        private readonly ContentControl _contentControl;

        public NavigationService(ContentControl contentControl)
        {
            _contentControl = contentControl ?? throw new ArgumentNullException(nameof(contentControl));
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
            if (!_viewsMap.TryGetValue(viewName, out Type viewType))
                throw new ArgumentException($"View '{viewName}' is not registered", nameof(viewName));

            UserControl view = (UserControl)Activator.CreateInstance(viewType);

            if (_contentControl.Content is UserControl currentView)
            {
                _navigationStack.Push(currentView);
                _parametersStack.Push(parameter);
            }

            if (view.DataContext is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(parameter);
            }

            _contentControl.Content = view;
        }

        public void GoBack()
        {
            if (!CanGoBack)
                return;

            UserControl previousView = _navigationStack.Pop();
            object previousParameter = _parametersStack.Pop();

            if (previousView.DataContext is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(previousParameter);
            }

            _contentControl.Content = previousView;
        }

        public bool CanGoBack => _navigationStack.Count > 0;
    }

    public interface INavigationAware
    {
        void OnNavigatedTo(object parameter);
        void OnNavigatedFrom();
    }

    public class NavigationAwareViewModel : ViewModelBase, INavigationAware
    {
        public virtual void OnNavigatedTo(object parameter)
        {
            // Default implementation does nothing
        }

        public virtual void OnNavigatedFrom()
        {
            // Default implementation does nothing
        }
    }
}