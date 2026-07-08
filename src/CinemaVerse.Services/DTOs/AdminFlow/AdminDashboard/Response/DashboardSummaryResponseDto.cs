namespace CinemaVerse.Services.DTOs.AdminFlow.AdminDashboard.Response
{

    public class DashboardSummaryResponseDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal? TotalRevenueChangePercent { get; set; }
        public decimal TotalBookings { get; set; }
        public decimal? TotalBookingsChangePercent { get; set; }
        public decimal ActiveUsers { get; set; }
        public decimal? ActiveUsersChangePercent { get; set; }
        public decimal OccupancyRate { get; set; }
        public decimal? OccupancyRateChangePercent { get; set; }
        public Dictionary<string, decimal> MonthlyRevenue { get; set; } = new();
        public Dictionary<string, decimal> WeeklyBookings { get; set; } = new();

        public static DashboardSummaryResponseDto From(DashboardSummaryDto s) => new()
        {
            TotalRevenue = s.TotalRevenue.Value,
            TotalRevenueChangePercent = s.TotalRevenue.PercentChange,
            TotalBookings = s.TotalBookings.Value,
            TotalBookingsChangePercent = s.TotalBookings.PercentChange,
            ActiveUsers = s.ActiveUsers.Value,
            ActiveUsersChangePercent = s.ActiveUsers.PercentChange,
            OccupancyRate = s.OccupancyRate.Value,
            OccupancyRateChangePercent = s.OccupancyRate.PercentChange,
            MonthlyRevenue = s.MonthlyRevenue,
            WeeklyBookings = s.WeeklyBookings
        };
    }
}
