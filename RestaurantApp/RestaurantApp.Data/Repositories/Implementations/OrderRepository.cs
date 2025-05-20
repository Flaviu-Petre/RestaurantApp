using Microsoft.EntityFrameworkCore;
using RestaurantApp.Core.Models;
using RestaurantApp.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantApp.Data.Repositories.Implementations
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(RestaurantDbContext context) : base(context)
        {
        }

        public async Task<Order> GetWithDetailsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order> GetWithUserAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order> GetWithFullDetailsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Dish)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Menu)
                        .ThenInclude(m => m.MenuDishes)
                            .ThenInclude(md => md.Dish)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order> GetByOrderCodeAsync(string orderCode)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Dish)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Menu)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Dish)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Menu)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetUserActiveOrdersAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Dish)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Menu)
                .Where(o => o.UserId == userId &&
                           (o.Status == OrderStatus.Registered ||
                            o.Status == OrderStatus.InPreparation ||
                            o.Status == OrderStatus.OutForDelivery))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Dish)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Menu)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> CountUserOrdersInPeriodAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .CountAsync(o => o.UserId == userId &&
                                o.OrderDate >= startDate &&
                                o.OrderDate <= endDate);
        }

        public async Task UpdateOrderStatusAsync(int id, OrderStatus newStatus)
        {
            var order = await GetByIdAsync(id);
            if (order != null)
            {
                order.Status = newStatus;
                await UpdateAsync(order);
            }
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Dish)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Menu)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
    }
}