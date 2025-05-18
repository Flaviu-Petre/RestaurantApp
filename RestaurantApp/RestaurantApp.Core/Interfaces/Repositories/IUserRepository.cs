using RestaurantApp.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        // Get user with related entities
        Task<User> GetWithOrdersAsync(int id);

        // Find user by email
        Task<User> GetByEmailAsync(string email);

        // Check if email exists
        Task<bool> EmailExistsAsync(string email);
    }
}