using CinemaVerse.Services.DTOs.AdminFlow.AdminDashboard.Response;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminDashboard
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
        Task<decimal> GetTotalRevenue();
        Task<decimal> GetTotalBookings();
        Task<decimal> GetActiveUsers();
        Task<decimal> CalculateOccupancyRate();
        Task<Dictionary<string, decimal>> GetMonthlyRevenue();
        Task<Dictionary<string, decimal>> GetWeeklyBookings();
    }
}
