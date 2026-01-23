using CinemaVerse.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Data.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int MovieShowTimeId { get; set; }
        public BookingStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public ApplicationUser User { get; set; } = null!;
        public MovieShowTime MovieShowTime { get; set; } = null!;
        public ICollection<BookingPayment> BookingPayments { get; set; } = new List<BookingPayment>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public List<BookingSeat> BookingSeats { get; set; } = new();

      
    }
}
