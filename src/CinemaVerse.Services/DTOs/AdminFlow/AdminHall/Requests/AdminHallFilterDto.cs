namespace CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Requests
{
    public class AdminHallFilterDto
    {
        // Search & Filters
        public string? SearchTerm { get; set; }
        public string? HallNumber { get; set; }
        public string? Capacity { get; set; }
        public string? HallType { get; set; }
        public string? HallStatus { get; set; }

        // Sorting
        public string SortBy { get; set; } = "HallStatus";
        public string SortOrder { get; set; } = "desc"; // asc, desc

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
