using System.Collections.Generic;
using System.Linq;

namespace RestaurantApp.Core.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }

        // Navigation properties
        public virtual Category Category { get; set; }
        public virtual ICollection<MenuDish> MenuDishes { get; set; } = new List<MenuDish>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        // Calculate the price with discount based on contained dishes
        // Note: The actual discount percentage will be read from configuration
        public decimal CalculatePrice(decimal discountPercentage)
        {
            if (MenuDishes == null || !MenuDishes.Any())
                return 0;

            var totalPrice = MenuDishes.Sum(md => md.Dish.Price);
            var discountMultiplier = 1 - (discountPercentage / 100);

            return totalPrice * discountMultiplier;
        }
    }
}