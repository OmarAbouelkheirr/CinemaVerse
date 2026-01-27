using CinemaVerse.Data.Data;
using CinemaVerse.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace CinemaVerse.Data.Repositories.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<T> _logger;

        public Repository(AppDbContext Context, ILogger<T> Logger)
        {
            _context = Context;
            _dbSet = _context.Set<T>();
            _logger = Logger;
        }

        public async Task AddAsync(T entity)
        {
            try
            {
                _logger.LogInformation("Adding entity of type {EntityType}", typeof(T).Name);
                await _dbSet.AddAsync(entity);
                _logger.LogInformation("Entity of type {EntityType} added successfully", typeof(T).Name);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> Predicate)
        {
            try
            {
                _logger.LogInformation("Checking if any {EntityType} exists with predicate", typeof(T).Name);

                var Result = await _dbSet.AnyAsync(Predicate);
                _logger.LogInformation("{EntityType} exists check result: {Result}", typeof(T).Name, Result);
                return Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eror checking if any {EntityType} exists with predicate", typeof(T).Name);
                throw;
            }
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? Predicate = null)
        {
            try
            {
                _logger.LogInformation("Counting entities of type {EntityType}", typeof(T).Name);
                if (Predicate != null)
                {
                    var Count = await _dbSet.CountAsync(Predicate);
                    _logger.LogInformation("Counted {Count} entities of type {EntityType} matching predicate", Count, typeof(T).Name);
                    return Count;
                }
                else
                {
                    var Count = await _dbSet.CountAsync();
                    _logger.LogInformation("Counted {Count} entities of type {EntityType}", Count, typeof(T).Name);
                    return Count;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting entity of {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task DeleteAsync(T entity)
        {
            try
            {
                _logger.LogInformation("Deleting entity of type {EntityType}", typeof(T).Name);
                _dbSet.Remove(entity);
                _logger.LogInformation("Entity of type {EntityType} deleted successfully", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity of {EntityType} ", typeof(T).Name);
                throw;
            }
        }

        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>>? Predicate)
        {
            try
            {
                _logger.LogInformation("Checking if any {EntityType} exists with predicate", typeof(T).Name);
                if (Predicate != null)
                {
                    var Result = await _dbSet.Where(Predicate).ToListAsync();
                    _logger.LogInformation("{EntityType} exists check result: {Result}", typeof(T).Name, Result);
                    return Result;
                }
                else
                {
                    var Result = await _dbSet.ToListAsync();
                    _logger.LogInformation("{EntityType} exists check result: {Result}", typeof(T).Name, Result);
                    return Result;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eror checking if any {EntityType} exists with predicate", typeof(T).Name);
                throw;
            }
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> Predicate)
        {
            try
            {
                _logger.LogInformation("getting first or default {EntityType} with predicate", typeof(T).Name);
                var Result = await _dbSet.FirstOrDefaultAsync(Predicate);
                if (Result == null)
                    _logger.LogDebug("No {EntityType} found matching predicate", typeof(T).Name);
                else
                    _logger.LogDebug("Successfully retrieved {EntityType}", typeof(T).Name);
                return Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting first or default {EntityType} with predicate", typeof(T).Name);
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Getting all entities of type {EntityType}", typeof(T).Name);
                var Results = await _dbSet.ToListAsync();
                _logger.LogInformation("Retrieved {Count} entities of type {EntityType}", Results.Count, typeof(T).Name);
                return Results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all {EntityType} entities", typeof(T).Name);
                throw;
            }
        }

        public async Task<T?> GetByIdAsync(int Id)
        {
            try
            {
                _logger.LogInformation("Getting {EntityType} by ID: {Id}", typeof(T).Name, Id);
                var Result = await _dbSet.FindAsync(Id);
                if (Result == null)
                {
                    _logger.LogWarning("{EntityType} with ID: {Id} not found", typeof(T).Name, Id);
                }
                else
                {
                    _logger.LogInformation("{EntityType} with ID: {Id} retrieved successfully", typeof(T).Name, Id);
                }
                return Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {EntityType} by ID: {Id}", typeof(T).Name, Id);
                throw;
            }
        }

        public async Task<T?> GetByIdAsync(string Id)
        {
            try
            {
                _logger.LogInformation("Getting {EntityType} by ID: {Id}", typeof(T).Name, Id);
                var Result = await _dbSet.FindAsync(Id);
                if (Result == null)
                {
                    _logger.LogWarning("{EntityType} with ID: {Id} not found", typeof(T).Name, Id);
                }
                else
                {
                    _logger.LogInformation("{EntityType} with ID: {Id} retrieved successfully", typeof(T).Name, Id);
                }
                return Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {EntityType} by ID: {Id}", typeof(T).Name, Id);
                throw;
            }
        }

        public async Task UpdateAsync(T entity)
        {
            try
            {
                _logger.LogInformation("Updating entity of type {EntityType}", typeof(T).Name);
                _dbSet.Update(entity);
                _logger.LogInformation("Entity of type {EntityType} updated successfully", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public IQueryable<T> GetQueryable()
        {
            try
            {
                _logger.LogDebug("Creating queryable for {EntityType}", typeof(T).Name);
                return _dbSet.AsQueryable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating queryable for {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<int> CountAsync(IQueryable<T> query)
        {
            try
            {
                _logger.LogInformation("Counting entities from custom query for type {EntityType}", typeof(T).Name);
                var count = await query.CountAsync();
                _logger.LogInformation("Counted {Count} entities of type {EntityType} from custom query", count, typeof(T).Name);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting entities from custom query for type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<List<T>> GetPagedAsync(
            IQueryable<T> query,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy,
            int skip,
            int take,
            string? includeProperties = null)
        {
            try
            {
                _logger.LogInformation("Getting paged entities of type {EntityType} (Skip: {Skip}, Take: {Take})", typeof(T).Name, skip, take);

                // Apply eager loading if specified
                if (!string.IsNullOrWhiteSpace(includeProperties))
                {
                    foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeProperty.Trim());
                    }
                }

                // Apply ordering if orderBy is provided
                IQueryable<T> orderedQuery = orderBy != null ? orderBy(query) : query;

                // Apply pagination
                var results = await orderedQuery
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} paged entities of type {EntityType}", results.Count, typeof(T).Name);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged entities of type {EntityType}", typeof(T).Name);
                throw;
            }
        }
    }
}
