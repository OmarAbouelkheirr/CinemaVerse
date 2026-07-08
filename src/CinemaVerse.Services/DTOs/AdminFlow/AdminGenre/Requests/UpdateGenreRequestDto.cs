using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminGenre.Requests
{
    public class UpdateGenreRequestDto
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Genre name must be between 1 and 100 characters")]
        public string? GenreName { get; set; }
    }
}
