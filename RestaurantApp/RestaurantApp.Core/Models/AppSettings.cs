namespace RestaurantApp.Core.Models
{
    public class AppSettings
    {
        public decimal MenuDiscountPercentage { get; set; } // x% discount for menus
        public decimal OrderValueForFreeShipping { get; set; } // y lei, minimum order for free shipping
        public decimal ShippingCost { get; set; } // b lei, shipping cost for orders below threshold
        public int LoyaltyOrderCount { get; set; } // z orders
        public int LoyaltyTimePeriodDays { get; set; } // t time period
        public decimal LoyaltyDiscountPercentage { get; set; } // w% discount for loyal customers
        public decimal LowStockThreshold { get; set; } // c quantity, threshold for low stock alert
    }
}