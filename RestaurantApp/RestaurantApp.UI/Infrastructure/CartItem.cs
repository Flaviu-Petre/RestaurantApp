using System.Collections.Generic;

namespace RestaurantApp.UI.Infrastructure
{
    public interface ICartService
    {
        void AddDish(int dishId, int quantity);
        void AddMenu(int menuId, int quantity);
        void UpdateQuantity(int itemId, bool isDish, int newQuantity);
        void RemoveItem(int itemId, bool isDish);
        void ClearCart();
        List<CartItem> GetCartItems();
        decimal GetSubtotal();
        decimal GetShippingCost();
        decimal GetDiscount();
        decimal GetTotal();
        int GetItemCount();
        bool HasItems();
    }

    public class CartItem
    {
        public int Id { get; set; }
        public bool IsDish { get; set; } // true for dish, false for menu
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public string ImageUrl { get; set; }
    }
}