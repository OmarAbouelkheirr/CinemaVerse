namespace CinemaVerse.Services.DTOs.AdminFlow.AdminGenre.Response
{
    public class GenreDetailsDto
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; } = string.Empty;
        public int MoviesCount { get; set; }
    }
}
