using System.ComponentModel.DataAnnotations;
using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminBooking.Requests
{
    public class UpdateBookingStatusRequestDto
    {
        [Required(ErrorMessage = "Status is required")]
        [EnumDataType(typeof(BookingStatus), ErrorMessage = "Invalid booking status")]
        public BookingStatus Status { get; set; }
    }
}
