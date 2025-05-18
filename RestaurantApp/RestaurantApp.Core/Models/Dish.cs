using System.Collections.Generic;

namespace RestaurantApp.Core.Models
{
    public class Dish
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal PortionQuantity { get; set; } // Quantity per portion (e.g., 300g)
        public decimal TotalQuantity { get; set; } // Total quantity available in restaurant
        public int CategoryId { get; set; }

        // You can add a collection of image paths
        public virtual ICollection<DishImage> Images { get; set; } = new List<DishImage>();

        // Navigation properties
        public virtual Category Category { get; set; }
        public virtual ICollection<DishAllergen> DishAllergens { get; set; } = new List<DishAllergen>();
        public virtual ICollection<MenuDish> MenuDishes { get; set; } = new List<MenuDish>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}