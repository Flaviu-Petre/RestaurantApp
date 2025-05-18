using System.Collections.Generic;

namespace RestaurantApp.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string DeliveryAddress { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public UserRole Role { get; set; } = UserRole.Customer;

        // Navigation property
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    public enum UserRole
    {
        Customer = 1,
        Employee = 2
    }
}