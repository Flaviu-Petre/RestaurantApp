using RestaurantApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count);
        Task<Order> GetOrderByIdAsync(int id);
        Task<Order> GetOrderWithDetailsAsync(int id);
        Task<Order> GetOrderByCodeAsync(string orderCode);
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<IEnumerable<Order>> GetUserActiveOrdersAsync(int userId);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<Order> CreateOrderAsync(Order order, IEnumerable<OrderDetail> orderDetails);
        Task UpdateOrderStatusAsync(int id, OrderStatus newStatus);
        Task CancelOrderAsync(int id);
        Task<decimal> CalculateOrderDiscountAsync(Order order, decimal orderValueThreshold, int loyaltyOrderCount, int loyaltyTimePeriodDays, decimal discountPercentage);
        Task<decimal> CalculateShippingCostAsync(Order order, decimal freeShippingThreshold, decimal standardShippingCost);
    }
}