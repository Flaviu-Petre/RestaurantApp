using RestaurantApp.Core.Models;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<User> AuthenticateAsync(string email, string password);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<string> GeneratePasswordHashAsync(string password, string salt = null);
    }
}