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
        public string SeatId { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public TicketStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;                  
    }
}
