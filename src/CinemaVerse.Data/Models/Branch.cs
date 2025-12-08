namespace CinemaVerse.Data.Models
{
    public class Branch
    {
        public int Id { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string BranchLocation { get; set; } = string.Empty;

        public ICollection<Hall> Halls { get; set; } = new List<Hall>();
    }
}
