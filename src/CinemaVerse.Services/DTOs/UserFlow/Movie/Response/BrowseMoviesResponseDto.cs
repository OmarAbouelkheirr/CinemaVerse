using CinemaVerse.Services.DTOs.Movie.Flow;

namespace CinemaVerse.Services.DTOs.Movie.Response
{
    public class BrowseMoviesResponseDto
    {

        public List<MovieCardDto> Movies { get; set; } = new List<MovieCardDto>();
        public int Page { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    }
}
