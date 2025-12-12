
using CinemaVerse.Data.Data;
using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Data.Repositories.Implementations
{
    public class HallRepository : Repository<Hall>, IHallRepository
    {
        private readonly AppDbContext _context;

        public HallRepository(AppDbContext context, ILogger<Hall> logger)
    : base(context, logger)
        {
            _context = context;
        }

        public async Task<IEnumerable<Hall>> GetAvailableHallsByBranchIdAsync(int branchId)
        {
            try
            {
                _logger.LogInformation("Fetching available halls for  {BranchId}", branchId);

                var halls = await _context.Halls
                    .Where(h => h.BranchId == branchId && h.HallStatus == HallStatus.Available)
                    .ToListAsync();

                if (halls.Count == 0)
                    _logger.LogWarning("No available halls found for BranchId: {BranchId}", branchId);
                else
                    _logger.LogInformation("Retrieved {Count} available halls for BranchId: {BranchId}", halls.Count, branchId);

                return halls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available halls for BranchId: {BranchId}", branchId);
                throw;
            }
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
                    //.Include(h => h.MovieShowTimes.Where(st => st.ShowStartTime > DateTime.Now))
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

        public async Task<IEnumerable<Hall>> GetHallsWithDetailsByBranchAsync(int branchId)
        {
            try
            {
                _logger.LogInformation("Fetching halls for BranchId: {BranchId}", branchId);

                var halls = await _context.Halls
                    .Where(h => h.BranchId == branchId)
                    .Include(h => h.Seats)
                    .Include(h => h.MovieShowTimes)
                    .Include(h => h.Branch)
                    .ToListAsync();

                if (halls.Count == 0)
                    _logger.LogWarning("No halls found for BranchId: {BranchId}", branchId);
                else
                    _logger.LogInformation("Retrieved {Count} halls for BranchId: {BranchId}", halls.Count, branchId);

                return halls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching halls for BranchId: {BranchId}", branchId);
                throw;
            }
        }

        public async Task<IEnumerable<Hall>> GetHallsByBranchIdAsync(int branchId)
        {
            try
            {
                _logger.LogInformation("Fetching halls for BranchId = {BranchId}", branchId);

                var halls = await _context.Halls
                    .Where(h => h.BranchId == branchId)
                    .ToListAsync();

                if (halls.Count == 0)
                    _logger.LogWarning("No halls found for BranchId = {BranchId}", branchId);
                else
                    _logger.LogInformation("Retrieved {Count} halls for BranchId = {BranchId}", halls.Count, branchId);

                return halls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching halls for BranchId = {BranchId}", branchId);
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
