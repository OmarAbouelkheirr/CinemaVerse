using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.Movie.Flow
{
    public class MovieShowTimeDto
    {
        public int MovieShowTimeId { get; set; }
        public DateTime ShowStartTime { get; set; }
        public int HallId { get; set; }
        public string HallNumber { get; set; } = string.Empty;
        public HallType HallType { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string BranchLocation { get; set; } = string.Empty;
        public decimal TicketPrice { get; set; }
    }
}
