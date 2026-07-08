using System.ComponentModel.DataAnnotations;
using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminBooking.Requests
{
    public class UpdateBookingRequestDto
    {
        [Required(ErrorMessage = "Booking status is required")]
        [EnumDataType(typeof(BookingStatus), ErrorMessage = "Invalid booking status")]
        public BookingStatus NewStatus { get; set; }
    }
}
