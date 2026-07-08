using CinemaVerse.Data.Models;
using CinemaVerse.Services.DTOs.AdminFlow.AdminGenre.Response;

namespace CinemaVerse.Services.Mappers
{
    public static class GenreMapper
    {
        public static GenreDetailsDto ToGenreDetailsDto(Genre genre, int moviesCount)
        {
            return new GenreDetailsDto
            {
                GenreId = genre.Id,
                GenreName = genre.GenreName,
                MoviesCount = moviesCount
            };
        }
    }
}
