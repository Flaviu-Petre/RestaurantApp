using RestaurantApp.Core.Interfaces.Repositories;
using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Interfaces;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Services.Implementations
{
    public class AuthenticationService : BaseService, IAuthenticationService
    {
        private readonly IUserRepository _userRepository;

        public AuthenticationService(IRepositoryFactory repositoryFactory) : base(repositoryFactory)
        {
            _userRepository = repositoryFactory.Users;
        }

        public async Task<User> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return null;

            var passwordHash = await GeneratePasswordHashAsync(password, user.PasswordSalt);

            if (user.PasswordHash != passwordHash)
                return null;

            return user;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            var currentPasswordHash = await GeneratePasswordHashAsync(currentPassword, user.PasswordSalt);
            if (user.PasswordHash != currentPasswordHash)
                return false;

            // Generate new salt and hash for the new password
            string salt = Guid.NewGuid().ToString();
            string passwordHash = await GeneratePasswordHashAsync(newPassword, salt);

            user.PasswordSalt = salt;
            user.PasswordHash = passwordHash;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<string> GeneratePasswordHashAsync(string password, string salt = null)
        {
            // If no salt is provided, create a new one
            if (string.IsNullOrEmpty(salt))
            {
                salt = Guid.NewGuid().ToString();
            }

            // Combine password and salt
            string combined = password + salt;

            // Use SHA256 for hashing
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Compute hash
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(combined));

                // Convert to string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}