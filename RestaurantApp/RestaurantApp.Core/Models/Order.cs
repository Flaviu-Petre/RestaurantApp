using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantApp.Core.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } // Unique order code
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public decimal FoodCost { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalCost => FoodCost + ShippingCost - Discount;
        public int UserId { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        // Calculate the food cost based on order details
        public void CalculateFoodCost()
        {
            if (OrderDetails == null || !OrderDetails.Any())
            {
                FoodCost = 0;
                return;
            }

            FoodCost = OrderDetails.Sum(od => od.Quantity * od.UnitPrice);
        }
    }

    public enum OrderStatus
    {
        Registered = 1,
        InPreparation = 2,
        OutForDelivery = 3,
        Delivered = 4,
        Cancelled = 5
    }
}