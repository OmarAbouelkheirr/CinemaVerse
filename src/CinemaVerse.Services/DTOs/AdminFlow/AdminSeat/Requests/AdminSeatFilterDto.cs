namespace CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Requests
{
    public class AdminSeatFilterDto
    {
        // Search & Filters
        public string? SearchTerm { get; set; }
        public int? HallId { get; set; }

        // Sorting
        public string SortBy { get; set; } = "SeatLabel"; // SeatLabel, HallId
        public string SortOrder { get; set; } = "asc"; // asc, desc

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
