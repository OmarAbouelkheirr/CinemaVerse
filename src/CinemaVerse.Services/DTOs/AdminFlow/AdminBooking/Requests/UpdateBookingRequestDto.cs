using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminBooking.Requests
{
    public class UpdateBookingRequestDto
    {
        public BookingStatus NewStatus { get; set; } 
    }
}
