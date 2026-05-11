namespace SyncMed.Data.Repositories;

/// <summary>
/// Generic repository interface for basic CRUD operations
/// </summary>
public interface IGenericRepository<T> where T : class
{
    Task<IList<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task SaveChangesAsync();
}
