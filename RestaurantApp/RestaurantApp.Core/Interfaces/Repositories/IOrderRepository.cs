using RestaurantApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Interfaces.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        // Get order with related entities
        Task<Order> GetWithDetailsAsync(int id);
        Task<Order> GetWithUserAsync(int id);
        Task<Order> GetWithFullDetailsAsync(int id);
        Task<Order> GetByOrderCodeAsync(string orderCode);

        // Get orders by user
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<IEnumerable<Order>> GetUserActiveOrdersAsync(int userId);

        // Get orders by status
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);

        // Get recent orders
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count);

        // Count orders for user in time period
        Task<int> CountUserOrdersInPeriodAsync(int userId, DateTime startDate, DateTime endDate);

        // Update order status
        Task UpdateOrderStatusAsync(int id, OrderStatus newStatus);

        Task<IEnumerable<Order>> GetAllOrdersAsync();
    }
}