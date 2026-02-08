namespace CinemaVerse.Data.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        Task<decimal> GetTotalRevenue();
        Task<decimal> GetTotalBookings();
        Task<decimal> GetActiveUsers();
        Task<decimal> CalculateOccupancyRate();
        Task<Dictionary<string, decimal>> GetMonthlyRevenue();
        Task<Dictionary<string, decimal>> GetWeeklyBookings();
    }
}
