using RestaurantApp.Core.Interfaces.Repositories;

namespace RestaurantApp.Core.Services.Implementations
{
    public abstract class BaseService
    {
        protected readonly IRepositoryFactory _repositoryFactory;

        protected BaseService(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }
    }
}