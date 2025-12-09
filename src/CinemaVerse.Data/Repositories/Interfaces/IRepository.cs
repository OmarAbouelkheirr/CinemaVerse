using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Data.Repositories.Interfaces
{
    public interface IRepository <T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int Id);
        Task<T?> GetByIdAsync(string Id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<int> CountAsync(Expression<Func<T,bool>>? Predicate = null);
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>>? Predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> Predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> Predicate);


    }
}
