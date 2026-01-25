namespace CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Requests
{
    public class AdminBranchFilterDto
    {
        // Search & Filters
        public string? SearchTerm { get; set; }
        public string? BranchName { get; set; }
        public string? Location { get; set; }

        // Sorting
        public string SortBy { get; set; } = "BranchName"; 
        public string SortOrder { get; set; } = "desc"; // asc, desc

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
