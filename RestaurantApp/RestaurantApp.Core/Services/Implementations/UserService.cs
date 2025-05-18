using RestaurantApp.Core.Interfaces.Repositories;
using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Services.Implementations
{
    public class UserService : BaseService, IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationService _authenticationService;

        public UserService(
            IRepositoryFactory repositoryFactory,
            IAuthenticationService authenticationService) : base(repositoryFactory)
        {
            _userRepository = repositoryFactory.Users;
            _authenticationService = authenticationService;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User> GetUserWithOrdersAsync(int id)
        {
            return await _userRepository.GetWithOrdersAsync(id);
        }

        public async Task<User> CreateUserAsync(User user, string password)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            // Check if email already exists
            bool emailExists = await _userRepository.EmailExistsAsync(user.Email);
            if (emailExists)
                throw new InvalidOperationException($"A user with email {user.Email} already exists");

            // Generate random salt and hash the password
            string salt = Guid.NewGuid().ToString();
            string passwordHash = await _authenticationService.GeneratePasswordHashAsync(password, salt);

            user.PasswordSalt = salt;
            user.PasswordHash = passwordHash;

            return await _userRepository.AddAsync(user);
        }

        public async Task UpdateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // Get the existing user to keep password info
            var existingUser = await _userRepository.GetByIdAsync(user.Id);
            if (existingUser == null)
                throw new InvalidOperationException($"User with ID {user.Id} not found");

            // Update user properties but keep password hash and salt
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.DeliveryAddress = user.DeliveryAddress;
            existingUser.Role = user.Role;

            // Only update email if it changed and the new email doesn't exist
            if (existingUser.Email != user.Email)
            {
                bool emailExists = await _userRepository.EmailExistsAsync(user.Email);
                if (emailExists)
                    throw new InvalidOperationException($"A user with email {user.Email} already exists");

                existingUser.Email = user.Email;
            }

            await _userRepository.UpdateAsync(existingUser);
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new InvalidOperationException($"User with ID {id} not found");

            await _userRepository.DeleteAsync(user);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userRepository.EmailExistsAsync(email);
        }
    }
}