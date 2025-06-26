using RestAPI.Helpers;

namespace RestAPI.Repositories.Interfaces;

public interface IBaseRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task<T> CreateAsync(T entity);
    Task<T?> UpdateAsync(string id, T entity);
    Task<T?> DeleteAsync(string id);
    Task<PaginationResult<T>> GetAllAsyncWithPagination(QueryParams query);
}