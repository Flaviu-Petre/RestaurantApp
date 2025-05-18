using System.Collections.Generic;

namespace RestaurantApp.Core.Models
{
    public class Allergen
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Navigation property
        public virtual ICollection<DishAllergen> DishAllergens { get; set; } = new List<DishAllergen>();
    }
}