using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminUser.Requests
{
    public class AdminUserFilterDto
    {
        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        public string? SortBy { get; set; } // Email, FullName, CreatedAt, LastUpdatedAt
        public string? SortOrder { get; set; } // asc, desc

        // Filters
        public string? SearchTerm { get; set; } // Search by Email, FirstName, LastName
        public bool? IsActive { get; set; }
        public bool? IsEmailConfirmed { get; set; }
        public Genders? Gender { get; set; }
        public string? City { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? DateOfBirthFrom { get; set; }
        public DateTime? DateOfBirthTo { get; set; }
    }
}
