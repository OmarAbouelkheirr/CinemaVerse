namespace CinemaVerse.Services.DTOs.AdminFlow.AdminDashboard.Response
{
    public class ChartDataDto
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Data { get; set; } = new();
    }
}
