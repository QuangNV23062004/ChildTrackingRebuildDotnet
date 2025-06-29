using RestAPI.Helpers;
using RestAPI.Models.SubModels;

namespace RestAPI.Repositories.Interfaces;

public interface IBaseRepository<T>
    where T : class
{
    Task<IEnumerable<T>> GetAllAsync(PopulationModel[]? populationParams = null);
    Task<T?> GetByIdAsync(string id, PopulationModel[]? populationParams = null);
    Task<T> CreateAsync(T entity);
    Task<T?> UpdateAsync(string id, T entity);
    Task<T?> DeleteAsync(string id);
    Task<PaginationResult<T>> GetAllAsyncWithPagination(
        QueryParams query,
        PopulationModel[]? populationParams = null
    );
}
