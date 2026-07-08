using static CinemaVerse.Data.Models.Booking;

namespace CinemaVerse.Data.Models
{
    public class Seat
    {
        public int Id { get; set; }
        public string SeatLabel { get; set; } = string.Empty;
        public int HallId { get; set; }

        // Navigation Properties
        public Hall Hall { get; set; } = null!;
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public List<BookingSeat> BookingSeats { get; set; } = new();

    }
}
