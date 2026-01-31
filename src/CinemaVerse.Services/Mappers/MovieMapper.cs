using CinemaVerse.Data.Models;
using CinemaVerse.Services.DTOs.UserFlow.Movie.Flow;

namespace CinemaVerse.Services.Mappers
{
    public static class MovieMapper
    {
        public static MovieDetailsDto ToMovieDetailsDto(Movie movie)
        {
            return new MovieDetailsDto
            {
                MovieId = movie.Id,
                MovieName = movie.MovieName,
                MovieDescription = movie.MovieDescription,
                MovieDuration = movie.MovieDuration,
                ReleaseDate = movie.ReleaseDate,
                MovieAgeRating = movie.MovieAgeRating,
                MovieRating = movie.MovieRating,
                TrailerUrl = movie.TrailerUrl ?? string.Empty,
                MoviePoster = movie.MoviePoster ?? string.Empty,
                Status = movie.Status,
                CastMembers = movie.CastMembers?
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new CastMemberDto
                    {
                        Id = c.Id,
                        PersonName = c.PersonName,
                        ImageUrl = c.ImageUrl,
                        RoleType = c.RoleType,
                        CharacterName = c.CharacterName,
                        DisplayOrder = c.DisplayOrder,
                        IsLead = c.IsLead
                    }).ToList() ?? new List<CastMemberDto>(),
                Genres = movie.MovieGenres
                    .Where(mg => mg.Genre != null)
                    .Select(mg => new GenreDto
                    {
                        GenreId = mg.Genre!.Id,
                        Name = mg.Genre.GenreName
                    }).ToList(),
                Images = movie.MovieImages?.Select(mi => new MovieImageDto
                {
                    Id = mi.Id,
                    ImageUrl = mi.ImageUrl
                }).ToList() ?? new List<MovieImageDto>(),
                ShowTimes = movie.MovieShowTimes?
                    .Where(ms => ms.Hall != null && ms.Hall.Branch != null)
                    .Select(ms => ShowtimeMapper.ToMovieShowTimeDto(ms))
                    .ToList() ?? new List<MovieShowTimeDto>()
            };
        }

        public static MovieCardDto ToMovieCardDto(Movie movie)
        {
            return new MovieCardDto
            {
                MovieId = movie.Id,
                MovieName = movie.MovieName,
                MoviePosterImageUrl = movie.MoviePoster ?? string.Empty,
                MovieDuration = movie.MovieDuration.ToString(@"hh\:mm"),
                Genres = movie.MovieGenres?.Select(mg => mg.Genre?.GenreName ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>(),
                MovieRating = movie.MovieRating
            };
        }
    }
}
