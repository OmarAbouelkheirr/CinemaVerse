using CinemaVerse.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Requests
{
    public class CreateHallRequestDto
    {
        [Required(ErrorMessage = "Branch ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Branch ID must be greater than 0")]
        public int BranchId { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(10, 1000, ErrorMessage = "Capacity must be between 10 and 1000 seats")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Hall number is required")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Hall number must be between 1 and 20 characters")]
        [RegularExpression(@"^[A-Za-z0-9-]+$", ErrorMessage = "Hall number can only contain letters, numbers, and hyphens")]
        public string HallNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hall status is required")]
        [EnumDataType(typeof(HallStatus), ErrorMessage = "Invalid hall status")]
        public HallStatus HallStatus { get; set; }

        [Required(ErrorMessage = "Hall type is required")]
        [EnumDataType(typeof(HallType), ErrorMessage = "Invalid hall type")]
        public HallType HallType { get; set; }
    }
}
