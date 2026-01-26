using CinemaVerse.Data.Enums;
using CinemaVerse.Services.DTOs.Booking.Helpers;
using CinemaVerse.Services.DTOs.HallSeat.Responses;
using CinemaVerse.Services.DTOs.Ticket.Response;

namespace CinemaVerse.Services.DTOs.Common
{
    public class BookingDetailsDto
    {
        public int BookingId { get; set; }
        public BookingStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public ShowtimeDto Showtime { get; set; } = new();
        public List<SeatDto> BookedSeats { get; set; } = new();
        public List<TicketDetailsDto> Tickets { get; set; } = new();
    }
}
