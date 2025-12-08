using CinemaVerse.Data.Enums;

namespace CinemaVerse.Data.Models
{
    public class Hall
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string HallNumber { get; set; } = string.Empty;
        //public string HallPlace { get; set;} = string.Empty;
        public int Capacity { get; set; }
        public HallStatus HallStatus { get; set; } = HallStatus.Available;
        public HallType HallType { get; set; }
    }
}
