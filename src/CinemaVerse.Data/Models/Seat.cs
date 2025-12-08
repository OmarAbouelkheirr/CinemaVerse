namespace CinemaVerse.Data.Models
{
    public class Seat
    {
        public int Id { get; set; }
        public string SeatLabel { get; set; } = string.Empty;
        public int HallId { get; set; }
    }
}
