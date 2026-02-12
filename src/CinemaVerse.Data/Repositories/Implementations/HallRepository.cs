
using CinemaVerse.Data.Data;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Data.Repositories.Implementations
{
    public class HallRepository : Repository<Hall>, IHallRepository
    {
        private new readonly AppDbContext _context;

        public HallRepository(AppDbContext context, ILogger<Hall> logger)
    : base(context, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

        }

        public async Task<Hall?> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching hall with details. Id: {HallId}", id);

                var hall = await _context.Halls
                    .Include(h => h.Branch)
                    .Include(h => h.Seats)
                     .Include(h => h.MovieShowTimes)
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (hall == null)
                    _logger.LogWarning("Hall not found. Id: {HallId}", id);

                return hall;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching hall with Id: {HallId}", id);
                throw;
            }
        }

        public async Task<bool> IsHallNumberExistsAsync(int branchId, string hallNumber)
        {
            try
            {
                _logger.LogInformation(
                    "Checking if HallNumber '{HallNumber}' exists in BranchId {BranchId}",
                    hallNumber, branchId);

                hallNumber = hallNumber.ToLower();

                bool exists = await _context.Halls.AnyAsync(h =>
                    h.BranchId == branchId &&
                    h.HallNumber.ToLower() == hallNumber);

                if (exists)
                {
                    _logger.LogWarning(
                        "Hall number '{HallNumber}' already exists in BranchId {BranchId}",
                        hallNumber, branchId);
                }
                else
                {
                    _logger.LogInformation(
                        "Hall number '{HallNumber}' is available for BranchId {BranchId}",
                        hallNumber, branchId);
                }

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking hall number '{HallNumber}' in BranchId {BranchId}",
                    hallNumber, branchId);
                throw;
            }
        }



    }
}
