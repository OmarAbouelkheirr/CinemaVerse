using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.AdminFlow.AdminDashboard.Response;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Services.Implementations.Admin
{
    public class AdminDashboardService : IAdminDashboard
    {
        private const int PeriodDays = 30;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminDashboardService> _logger;

        public AdminDashboardService(IUnitOfWork unitOfWork, ILogger<AdminDashboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var now = DateTime.UtcNow;
            var currentEnd = now;
            var currentStart = now.AddDays(-PeriodDays);
            var previousEnd = currentStart;
            var previousStart = now.AddDays(-PeriodDays * 2);

            var dashboard = _unitOfWork.Dashboard;

            var revenueCurrentTask = dashboard.GetTotalRevenueForPeriodAsync(currentStart, currentEnd);
            var revenuePreviousTask = dashboard.GetTotalRevenueForPeriodAsync(previousStart, previousEnd);
            var bookingsCurrentTask = dashboard.GetTotalBookingsForPeriodAsync(currentStart, currentEnd);
            var bookingsPreviousTask = dashboard.GetTotalBookingsForPeriodAsync(previousStart, previousEnd);
            var activeUsersCurrentTask = dashboard.GetActiveUsersForPeriodAsync(currentStart, currentEnd);
            var activeUsersPreviousTask = dashboard.GetActiveUsersForPeriodAsync(previousStart, previousEnd);
            var occupancyCurrentTask = dashboard.GetOccupancyRateForPeriodAsync(currentStart, currentEnd);
            var occupancyPreviousTask = dashboard.GetOccupancyRateForPeriodAsync(previousStart, previousEnd);
            var monthlyRevenueTask = dashboard.GetMonthlyRevenue();
            var weeklyBookingsTask = dashboard.GetWeeklyBookings();

            await Task.WhenAll(
                revenueCurrentTask, revenuePreviousTask,
                bookingsCurrentTask, bookingsPreviousTask,
                activeUsersCurrentTask, activeUsersPreviousTask,
                occupancyCurrentTask, occupancyPreviousTask,
                monthlyRevenueTask, weeklyBookingsTask);

            return new DashboardSummaryDto
            {
                TotalRevenue = ToKpi(await revenueCurrentTask, await revenuePreviousTask),
                TotalBookings = ToKpi(await bookingsCurrentTask, await bookingsPreviousTask),
                ActiveUsers = ToKpi(await activeUsersCurrentTask, await activeUsersPreviousTask),
                OccupancyRate = ToKpi(await occupancyCurrentTask, await occupancyPreviousTask),
                MonthlyRevenue = await monthlyRevenueTask,
                WeeklyBookings = await weeklyBookingsTask
            };
        }

        private static DashboardKpiDto ToKpi(decimal current, decimal previous)
        {
            decimal? percentChange = previous == 0 ? 0 : Math.Round((current - previous) / previous * 100, 2);
            return new DashboardKpiDto { Value = current, PercentChange = percentChange };
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
