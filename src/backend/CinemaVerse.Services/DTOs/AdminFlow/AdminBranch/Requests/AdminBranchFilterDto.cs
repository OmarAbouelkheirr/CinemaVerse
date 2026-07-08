using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Requests
{
    public class AdminBranchFilterDto
    {
        [StringLength(200, ErrorMessage = "Search term must not exceed 200 characters")]
        public string? SearchTerm { get; set; }
        [StringLength(200, ErrorMessage = "Branch name must not exceed 200 characters")]
        public string? BranchName { get; set; }
        [StringLength(500, ErrorMessage = "Location must not exceed 500 characters")]
        public string? Location { get; set; }

        [StringLength(50, ErrorMessage = "Sort by must not exceed 50 characters")]
        public string SortBy { get; set; } = "BranchName";
        [StringLength(10, ErrorMessage = "Sort order must be 'asc' or 'desc'")]
        public string SortOrder { get; set; } = "desc";

        [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
        public int Page { get; set; } = 1;
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 20;
    }
}
