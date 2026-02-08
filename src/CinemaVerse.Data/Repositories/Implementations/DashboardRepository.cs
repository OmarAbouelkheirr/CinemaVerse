using CinemaVerse.Data.Data;
using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Data.Repositories.Implementations
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DashboardRepository> _logger;
        public DashboardRepository(AppDbContext context, ILogger<DashboardRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<decimal> CalculateOccupancyRate()
        {
            try
            {
                _logger.LogInformation("Calculating occupancy rate for the dashboard.");
                var rates = await _context.Bookings
                    .Where(b => b.Status == BookingStatus.Confirmed)
                    .GroupBy(b => b.MovieShowTimeId)
                    .Select(g => new
                    {
                        ShowTimeId = g.Key,
                        OccupiedSeats = g.Sum(b => b.BookingSeats.Count),
                        TotalSeats = _context.MovieShowTimes
                            .Where(ms => ms.Id == g.Key)
                            .SelectMany(ms => ms.Hall.Seats)
                            .Count()
                    })
                    .Select(x => x.TotalSeats > 0 ? (decimal)x.OccupiedSeats / x.TotalSeats * 100 : 0m)
                    .ToListAsync();
                var occupancyRate = rates.Count > 0 ? rates.Average() : 0m;
                _logger.LogInformation("Occupancy rate calculated: {OccupancyRate}%", occupancyRate);
                return occupancyRate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating occupancy rate for the dashboard.");
                throw;
            }
        }

        public async Task<decimal> GetActiveUsers()
        {
            try
            {
                _logger.LogInformation("Calculating active users for the dashboard.");
                var activeUsers = await _context.Users.CountAsync(u => u.Bookings.Any(b => b.Status == BookingStatus.Confirmed) && u.IsActive);
                _logger.LogInformation("Active users calculated: {ActiveUsersCount}", activeUsers);
                return activeUsers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating active users for the dashboard.");
                throw;
            }
        }

        public async Task<Dictionary<string, decimal>> GetMonthlyRevenue()
        {
            try
            {
                _logger.LogInformation("Calculating monthly revenue for the dashboard.");
                var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
                var startOfSixMonthsAgo = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var list = await _context.Bookings
                    .Where(b => b.Status == BookingStatus.Confirmed && b.CreatedAt >= startOfSixMonthsAgo)
                    .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Revenue = g.Sum(b => (decimal?)b.TotalAmount) ?? 0m
                    })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToListAsync();
                var monthlyRevenue = list.ToDictionary(x => $"{x.Year}-{x.Month:D2}", x => x.Revenue);
                _logger.LogInformation("Monthly revenue calculation initiated.");
                return monthlyRevenue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating monthly revenue for the dashboard.");
                throw;
            }
        }

        public async Task<decimal> GetTotalBookings()
        {
            try
            {
                _logger.LogInformation("Calculating total bookings for the dashboard.");
                var totalBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Confirmed);
                _logger.LogInformation("Total bookings calculated: {TotalBookingsCount}", totalBookings);
                return totalBookings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total bookings for the dashboard.");
                throw;
            }
        }

        public async Task<decimal> GetTotalRevenue()
        {
            try
            {
                _logger.LogInformation("Calculating total revenue for the dashboard.");
                var totalRevenue = await _context.Bookings
                    .Where(b => b.Status == BookingStatus.Confirmed)
                    .SumAsync(b => (decimal?)b.TotalAmount) ?? 0m;
                _logger.LogInformation("Total revenue calculated: {TotalRevenueAmount}", totalRevenue);
                return totalRevenue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total revenue for the dashboard.");
                throw;
            }
        }

        public async Task<Dictionary<string, decimal>> GetWeeklyBookings()
        {
            try
            {
                _logger.LogInformation("Calculating weekly bookings for the dashboard.");
                var start = DateTime.UtcNow.Date.AddDays(-6);
                var dailyCounts = await _context.Bookings
                    .Where(b => b.CreatedAt >= start && b.Status == BookingStatus.Confirmed)
                    .GroupBy(b => b.CreatedAt.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .ToListAsync();
                var dict = new Dictionary<string, decimal>();
                for (var d = 0; d <= 6; d++)
                {
                    var date = start.AddDays(d);
                    var dayName = date.ToString("ddd");
                    var count = dailyCounts.FirstOrDefault(x => x.Date == date)?.Count ?? 0;
                    dict[dayName] = count;
                }
                _logger.LogInformation("Weekly bookings calculated for the dashboard.");
                return dict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating weekly bookings for the dashboard.");
                throw;
            }
        }
    }
}
