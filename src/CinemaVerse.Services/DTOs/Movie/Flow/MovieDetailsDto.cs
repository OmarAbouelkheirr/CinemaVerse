using System;
using System.Collections.Generic;

namespace CinemaVerse.Services.DTOs.Movie.Flow
{
    public class MovieDetailsDto
    {
        public int MovieId { get; set; }
        public string MovieName { get; set; } = string.Empty;
        public string MovieDescription { get; set; } = string.Empty;
        public TimeSpan MovieDuration { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public string MovieAgeRating { get; set; } = string.Empty;
        public decimal MovieRating { get; set; }
        public string TrailerUrl { get; set; } = string.Empty;

        public List<string> Cast { get; set; } = new();
        public List<GenreDto> Genres { get; set; } = new();
        public List<MovieImageDto> Images { get; set; } = new();
        public List<MovieShowTimeDto> ShowTimes { get; set; } = new();
    }

    public class GenreDto
    {
        public int GenreId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
