using CinemaVerse.Data.Models;

namespace CinemaVerse.Data.Repositories.Interfaces
{
    public interface IHallRepository : IRepository<Hall>
    {
        Task<Hall?> GetByIdWithDetailsAsync(int id);
        Task<bool> IsHallNumberExistsAsync(int branchId, string hallNumber);
    }
}
