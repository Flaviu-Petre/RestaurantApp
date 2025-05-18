namespace RestaurantApp.Core.Interfaces.Repositories
{
    public interface IRepositoryFactory
    {
        ICategoryRepository Categories { get; }
        IDishRepository Dishes { get; }
        IMenuRepository Menus { get; }
        IOrderRepository Orders { get; }
        IUserRepository Users { get; }
        IAllergenRepository Allergens { get; }
    }
}