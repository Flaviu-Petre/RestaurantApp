using RestaurantApp.Core.Interfaces.Data;
using RestaurantApp.Core.Interfaces.Repositories;
using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Services.Implementations
{
    public class OrderService : BaseService, IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IDishRepository _dishRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly IStoredProcedureExecutor _storedProcedureExecutor;
        public OrderService(
            IRepositoryFactory repositoryFactory,
            IStoredProcedureExecutor storedProcedureExecutor) : base(repositoryFactory)
        {
            _orderRepository = repositoryFactory.Orders;
            _dishRepository = repositoryFactory.Dishes;
            _menuRepository = repositoryFactory.Menus;
            _storedProcedureExecutor = storedProcedureExecutor;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count)
        {
            return await _orderRepository.GetRecentOrdersAsync(count);
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _orderRepository.GetByIdAsync(id);
        }

        public async Task<Order> GetOrderWithDetailsAsync(int id)
        {
            return await _orderRepository.GetWithFullDetailsAsync(id);
        }

        public async Task<Order> GetOrderByCodeAsync(string orderCode)
        {
            return await _orderRepository.GetByOrderCodeAsync(orderCode);
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await _orderRepository.GetUserOrdersAsync(userId);
        }

        public async Task<IEnumerable<Order>> GetUserActiveOrdersAsync(int userId)
        {
            return await _orderRepository.GetUserActiveOrdersAsync(userId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _orderRepository.GetOrdersByStatusAsync(status);
        }

        public async Task<Order> CreateOrderAsync(Order order, IEnumerable<OrderDetail> orderDetails)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (orderDetails == null || !orderDetails.Any())
                throw new ArgumentException("Order must contain at least one item", nameof(orderDetails));

            // Check if all items are available
            foreach (var detail in orderDetails)
            {
                if (detail.DishId.HasValue)
                {
                    var dish = await _dishRepository.GetByIdAsync(detail.DishId.Value);
                    if (dish == null || dish.TotalQuantity < 0)
                        throw new InvalidOperationException($"Dish with ID {detail.DishId} is not available");
                }
                else if (detail.MenuId.HasValue)
                {
                    var isAvailable = await IsMenuAvailableAsync(detail.MenuId.Value);
                    if (!isAvailable)
                        throw new InvalidOperationException($"Menu with ID {detail.MenuId} is not available");
                }
            }

            // Generate unique order code
            order.OrderCode = GenerateOrderCode();
            order.OrderDate = DateTime.Now;
            order.Status = OrderStatus.Registered;

            // Save order first
            var createdOrder = await _orderRepository.AddAsync(order);

            // Add order details
            foreach (var detail in orderDetails)
            {
                detail.OrderId = createdOrder.Id;

                // Set unit price based on the current price of the dish or menu
                if (detail.DishId.HasValue)
                {
                    var dish = await _dishRepository.GetByIdAsync(detail.DishId.Value);
                    detail.UnitPrice = dish.Price;
                }
                else if (detail.MenuId.HasValue)
                {
                    var menu = await _menuRepository.GetWithDishesAsync(detail.MenuId.Value);
                    detail.UnitPrice = menu.CalculatePrice(10); // 10% is a placeholder, should use configuration
                }
            }

            createdOrder.OrderDetails = orderDetails.ToList();
            createdOrder.CalculateFoodCost();

            await _orderRepository.UpdateAsync(createdOrder);

            return createdOrder;
        }

        public async Task UpdateOrderStatusAsync(int id, OrderStatus newStatus)
        {
            // Using stored procedure
            await _storedProcedureExecutor.UpdateOrderStatusAsync(id, newStatus);
        }

        public async Task CancelOrderAsync(int id)
        {
            var order = await _orderRepository.GetWithDetailsAsync(id);
            if (order == null)
                throw new InvalidOperationException($"Order with ID {id} not found");

            if (order.Status == OrderStatus.Delivered)
                throw new InvalidOperationException("Cannot cancel an order that has already been delivered");

            await UpdateOrderStatusAsync(id, OrderStatus.Cancelled);
        }

        public async Task<decimal> CalculateOrderDiscountAsync(
            Order order,
            decimal orderValueThreshold,
            int loyaltyOrderCount,
            int loyaltyTimePeriodDays,
            decimal discountPercentage)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            decimal discount = 0;

            // Check if order value exceeds threshold for discount
            if (order.FoodCost >= orderValueThreshold)
            {
                discount += order.FoodCost * (discountPercentage / 100);
            }

            // Check for loyalty discount (multiple orders in a time period)
            DateTime startDate = DateTime.Now.AddDays(-loyaltyTimePeriodDays);
            int userOrderCount = await _orderRepository.CountUserOrdersInPeriodAsync(
                order.UserId, startDate, DateTime.Now);

            if (userOrderCount >= loyaltyOrderCount)
            {
                // Additional loyalty discount
                discount += order.FoodCost * (discountPercentage / 100);
            }

            return discount;
        }

        public async Task<decimal> CalculateShippingCostAsync(
            Order order,
            decimal freeShippingThreshold,
            decimal standardShippingCost)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            // If order value exceeds threshold, shipping is free
            if (order.FoodCost >= freeShippingThreshold)
                return 0;

            return standardShippingCost;
        }

        private async Task<bool> IsMenuAvailableAsync(int id)
        {
            var menu = await _menuRepository.GetWithDishesAsync(id);
            if (menu == null)
                return false;

            // Check if all dishes in the menu are available
            foreach (var menuDish in menu.MenuDishes)
            {
                var dish = await _dishRepository.GetByIdAsync(menuDish.DishId);
                if (dish == null || dish.TotalQuantity <= 0)
                    return false;
            }

            return true;
        }

        private string GenerateOrderCode()
        {
            // Generate a unique order code
            // Format: ORD-YYYYMMDD-XXXXX where XXXXX is a random number
            string dateStr = DateTime.Now.ToString("yyyyMMdd");
            string randomPart = new Random().Next(10000, 99999).ToString();
            return $"ORD-{dateStr}-{randomPart}";
        }
    }
}