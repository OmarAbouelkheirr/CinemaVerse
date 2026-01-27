namespace CinemaVerse.Services.DTOs.AdminFlow.AdminShowtime.Requests
{
    public class AdminShowtimeFilterDto
    {
        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        public string? SortBy { get; set; } // showstarttime, price, moviename, hallnumber
        public string? SortOrder { get; set; } // asc, desc

        // Filters
        public string? SearchTerm { get; set; } // Search by movie name or hall number
        public int? MovieId { get; set; }
        public int? HallId { get; set; }
        public int? BranchId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
    }
}
