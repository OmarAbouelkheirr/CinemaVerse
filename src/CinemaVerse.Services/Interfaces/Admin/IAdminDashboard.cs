namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminDashboard
    {
        public Task<decimal> GetTotalRevenue();
        public Task<decimal> GetTotalBookings();
        public Task<decimal> GetActiveUsers();
        public Task<decimal> CalculateOccupancyRate();
        public Task<Dictionary<string, decimal>> GetMonthlyRevenue();
        public Task<Dictionary<string, decimal>> GetWeeklyBookings();


    }
}
