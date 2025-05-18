namespace RestaurantApp.Core.Models
{
    public class MenuDish
    {
        public int MenuId { get; set; }
        public int DishId { get; set; }
        public decimal QuantityInMenu { get; set; } // Custom quantity for this dish in this menu

        // Navigation properties
        public virtual Menu Menu { get; set; }
        public virtual Dish Dish { get; set; }
    }
}