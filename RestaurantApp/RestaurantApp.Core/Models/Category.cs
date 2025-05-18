using System.Collections.Generic;

namespace RestaurantApp.Core.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Navigation properties
        public virtual ICollection<Dish> Dishes { get; set; } = new List<Dish>();
        public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();
    }
}