namespace CinemaVerse.Data.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string MovieName { get; set; } = string.Empty;
        public string MovieDescription { get; set;} = string.Empty;
        public int MovieDuration {  get; set; }
        public string MovieCast{ get; set; } = string.Empty;
        public decimal Rating { get; set; } 
        public int MovieAgeRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string TrailerUrl { get; set; } = string.Empty;


        // ------------------------------------- //
    }
}
