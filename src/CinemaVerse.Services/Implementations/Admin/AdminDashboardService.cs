using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Services.Implementations.Admin
{
    public class AdminDashboardService : IAdminDashboard
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminDashboardService> _logger;

        public AdminDashboardService(IUnitOfWork unitOfWork, ILogger<AdminDashboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<decimal> CalculateOccupancyRate()
        {
            try
            {
                _logger.LogInformation("Calculating occupancy rate for the admin dashboard.");
                var occupancyRate = await _unitOfWork.Dashboard.CalculateOccupancyRate();
                _logger.LogInformation("Occupancy rate calculated: {OccupancyRate}%", occupancyRate);
                return occupancyRate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating occupancy rate for the admin dashboard.");
                throw;
            }
        }

        public async Task<decimal> GetActiveUsers()
        {
            try
            {
                _logger.LogInformation("Calculating active users for the admin dashboard.");
                var activeUsers = await _unitOfWork.Dashboard.GetActiveUsers();
                _logger.LogInformation("Active users calculated: {ActiveUsersCount}", activeUsers);
                return activeUsers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating active users for the admin dashboard.");
                throw;
            }
        }

        public async Task<Dictionary<string, decimal>> GetMonthlyRevenue()
        {
            try
            {
                _logger.LogInformation("Calculating monthly revenue for the admin dashboard.");
                var monthlyRevenue = await _unitOfWork.Dashboard.GetMonthlyRevenue();
                _logger.LogInformation("Monthly revenue calculated.");
                return monthlyRevenue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating monthly revenue for the admin dashboard.");
                throw;
            }
        }

        public async Task<decimal> GetTotalBookings()
        {
            try
            {
                _logger.LogInformation("Calculating total bookings for the admin dashboard.");
                var totalBookings = await _unitOfWork.Dashboard.GetTotalBookings();
                _logger.LogInformation("Total bookings calculated: {TotalBookingsCount}", totalBookings);
                return totalBookings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total bookings for the admin dashboard.");
                throw;
            }
        }

        public async Task<decimal> GetTotalRevenue()
        {
            try
            {
                _logger.LogInformation("Calculating total revenue for the admin dashboard.");
                var totalRevenue = await _unitOfWork.Dashboard.GetTotalRevenue();
                _logger.LogInformation("Total revenue calculated: {TotalRevenue}", totalRevenue);
                return totalRevenue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total revenue for the admin dashboard.");
                throw;
            }
        }

        public async Task<Dictionary<string, decimal>> GetWeeklyBookings()
        {
            try
            {
                _logger.LogInformation("Calculating weekly bookings for the admin dashboard.");
                var weeklyBookings = await _unitOfWork.Dashboard.GetWeeklyBookings();
                _logger.LogInformation("Weekly bookings calculated.");
                return weeklyBookings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating weekly bookings for the admin dashboard.");
                throw;
            }
        }
    }
}
