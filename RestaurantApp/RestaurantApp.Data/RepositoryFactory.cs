using RestaurantApp.Core.Interfaces.Repositories;
using RestaurantApp.Data.Repositories.Implementations;

namespace RestaurantApp.Data
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly RestaurantDbContext _context;

        private ICategoryRepository _categories;
        private IDishRepository _dishes;
        private IMenuRepository _menus;
        private IOrderRepository _orders;
        private IUserRepository _users;
        private IAllergenRepository _allergens;

        public RepositoryFactory(RestaurantDbContext context)
        {
            _context = context;
        }

        public ICategoryRepository Categories =>
            _categories ??= new CategoryRepository(_context);

        public IDishRepository Dishes =>
            _dishes ??= new DishRepository(_context);

        public IMenuRepository Menus =>
            _menus ??= new MenuRepository(_context);

        public IOrderRepository Orders =>
            _orders ??= new OrderRepository(_context);

        public IUserRepository Users =>
            _users ??= new UserRepository(_context);

        public IAllergenRepository Allergens =>
            _allergens ??= new AllergenRepository(_context);
    }
}