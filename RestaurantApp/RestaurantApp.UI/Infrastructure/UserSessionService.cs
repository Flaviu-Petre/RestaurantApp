using RestaurantApp.Core.Models;
using System;

namespace RestaurantApp.UI.Infrastructure
{
    public interface IUserSessionService
    {
        bool IsLoggedIn { get; }
        User CurrentUser { get; }
        bool IsEmployee { get; }
        bool IsCustomer { get; }

        void Login(User user);
        void Logout();

        event EventHandler UserLoggedIn;
        event EventHandler UserLoggedOut;
        event EventHandler UserChanged;
    }

    public class UserSessionService : IUserSessionService
    {
        private User _currentUser;

        public bool IsLoggedIn => CurrentUser != null;

        public User CurrentUser => _currentUser;

        public bool IsEmployee => IsLoggedIn && CurrentUser.Role == UserRole.Employee;

        public bool IsCustomer => IsLoggedIn && CurrentUser.Role == UserRole.Customer;

        public event EventHandler UserLoggedIn;
        public event EventHandler UserLoggedOut;
        public event EventHandler UserChanged; // Add this new event

        public void Login(User user)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            UserLoggedIn?.Invoke(this, EventArgs.Empty);
            UserChanged?.Invoke(this, EventArgs.Empty); // Notify user changed
        }

        public void Logout()
        {
            _currentUser = null;
            UserLoggedOut?.Invoke(this, EventArgs.Empty);
            UserChanged?.Invoke(this, EventArgs.Empty); // Notify user changed
        }
    }
}