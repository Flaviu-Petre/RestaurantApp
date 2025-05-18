namespace RestaurantApp.Core.Models
{
    public class DishImage
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public int DishId { get; set; }

        // Navigation property
        public virtual Dish Dish { get; set; }
    }
}