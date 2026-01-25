namespace CinemaVerse.Services.DTOs.AdminFlow.AdminGenre.Requests
{
    public class AdminGenreFilterDto
    {
        // Search & Filters
        public string? SearchTerm { get; set; }

        // Sorting
        public string SortBy { get; set; } = "GenreName"; // GenreName, Id
        public string SortOrder { get; set; } = "asc"; // asc, desc

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
