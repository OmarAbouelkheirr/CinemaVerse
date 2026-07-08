using CinemaVerse.Data.Models.Users;

namespace CinemaVerse.Data.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> IsEmailExistsAsync(string email);
    }
}
