using CinemaVerse.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Data.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public int SeatId { get; set; } 
        public decimal Price { get; set; }
        public TicketStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Booking Booking { get; set; } = null!;
        public Seat Seat { get; set; } = null!;
    }
}
