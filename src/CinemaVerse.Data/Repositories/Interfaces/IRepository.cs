using System.Linq.Expressions;

namespace CinemaVerse.Data.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int Id);
        Task<T?> GetByIdAsync(string Id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<int> CountAsync(Expression<Func<T, bool>>? Predicate = null);
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>>? Predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> Predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> Predicate);
        IQueryable<T> GetQueryable();
        Task<int> CountAsync(IQueryable<T> query);
        Task<List<T>> GetPagedAsync(
            IQueryable<T> query,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
            int skip,
            int take,
            string? includeProperties = null
        );
    }
}
