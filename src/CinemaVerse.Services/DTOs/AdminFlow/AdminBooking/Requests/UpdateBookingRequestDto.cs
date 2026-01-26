namespace CinemaVerse.Services.DTOs.AdminFlow.AdminBooking.Requests
{
    public class UpdateBookingRequestDto
    {
        public int BookingId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
    }
}
