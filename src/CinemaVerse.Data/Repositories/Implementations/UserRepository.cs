using CinemaVerse.Data.Data;
using CinemaVerse.Data.Models.Users;
using CinemaVerse.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Data.Repositories.Implementations
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private new readonly AppDbContext _context;

        public UserRepository(AppDbContext context, ILogger<User> logger)
            : base(context, logger)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation("Getting user by email: {Email}", email);

                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("Email is null or empty");
                    throw new ArgumentException("Email cannot be null or empty.", nameof(email));
                }

                var user = await _dbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (user == null)
                {
                    _logger.LogWarning("User with email {Email} not found", email);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved user with email {Email}", email);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", email);
                throw;
            }
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            try
            {
                _logger.LogInformation("Checking if email exists: {Email}", email);

                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("Email is null or empty");
                    return false;
                }

                var exists = await _dbSet
                    .AsNoTracking()
                    .AnyAsync(u => u.Email.ToLower() == email.ToLower());

                _logger.LogInformation("Email {Email} exists: {Exists}", email, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if email exists: {Email}", email);
                throw;
            }
        }
    }
}
