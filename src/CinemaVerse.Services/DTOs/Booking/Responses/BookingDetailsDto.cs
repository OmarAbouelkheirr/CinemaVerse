using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaVerse.Data.Enums;
using CinemaVerse.Services.DTOs.Booking.Helpers;
using CinemaVerse.Services.DTOs.Ticket.Response;

namespace CinemaVerse.Services.DTOs.Booking.Responses
{
    public class BookingDetailsDto
    {
        public int BookingId { get; set; }
        public BookingStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<TicketDetailsDto> Tickets { get; set; } = new();
        public ShowtimeDto Showtime { get; set; } = null!;
    }
}
