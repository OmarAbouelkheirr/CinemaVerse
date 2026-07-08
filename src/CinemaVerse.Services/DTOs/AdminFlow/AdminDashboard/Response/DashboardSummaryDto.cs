namespace CinemaVerse.Services.DTOs.AdminFlow.AdminDashboard.Response
{
    public class DashboardSummaryDto
    {
        public DashboardKpiDto TotalRevenue { get; set; } = new();
        public DashboardKpiDto TotalBookings { get; set; } = new();
        public DashboardKpiDto ActiveUsers { get; set; } = new();
        public DashboardKpiDto OccupancyRate { get; set; } = new();
        public Dictionary<string, decimal> MonthlyRevenue { get; set; } = new();
        public Dictionary<string, decimal> WeeklyBookings { get; set; } = new();
    }
}
