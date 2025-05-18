namespace RestaurantApp.Core.Models
{
    public class DishAllergen
    {
        public int DishId { get; set; }
        public int AllergenId { get; set; }

        // Navigation properties
        public virtual Dish Dish { get; set; }
        public virtual Allergen Allergen { get; set; }
    }
}