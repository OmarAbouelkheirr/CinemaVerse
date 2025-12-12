using CinemaVerse.Data.Models;

namespace CinemaVerse.Data.Repositories.Interfaces
{
    public interface IHallRepository : IRepository<Hall>
    {
        Task<Hall?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Hall>> GetHallsWithDetailsByBranchAsync(int branchId);
        Task<IEnumerable<Hall>> GetHallsByBranchIdAsync(int branchId);
        Task<bool> IsHallNumberExistsAsync(int branchId, string hallNumber);
        Task<IEnumerable<Hall>> GetAvailableHallsByBranchIdAsync(int branchId);

        //Task<IEnumerable<Hall>> GetAvailableHallsAsync();
        //Task<IEnumerable<Hall>> GetHallsWithSeatsAsync();
    }
}
