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
        public Guid UserId { get; set; }
        public int MovieShowTimeId { get; set; }
        public BookingStatus Status { get; set; }
        public decimal TotalAmount{ get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
