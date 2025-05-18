using Microsoft.EntityFrameworkCore;
using RestaurantApp.Core.Models;
using RestaurantApp.Core.Interfaces.Repositories;
using System.Threading.Tasks;

namespace RestaurantApp.Data.Repositories.Implementations
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(RestaurantDbContext context) : base(context)
        {
        }

        public async Task<User> GetWithOrdersAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderDetails)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }
    }
}