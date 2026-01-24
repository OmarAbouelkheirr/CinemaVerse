using CinemaVerse.Data.Models.Users;

namespace CinemaVerse.Data.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetUserWithBookingsAsync(int userId);
        Task<bool> IsEmailExistsAsync(string email);
    }
}
