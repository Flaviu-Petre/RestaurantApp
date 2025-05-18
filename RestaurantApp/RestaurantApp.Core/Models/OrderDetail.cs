namespace RestaurantApp.Core.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? DishId { get; set; } // Nullable because it can be either a dish or menu
        public int? MenuId { get; set; } // Nullable because it can be either a dish or menu
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Navigation properties
        public virtual Order Order { get; set; }
        public virtual Dish Dish { get; set; }
        public virtual Menu Menu { get; set; }
    }
}