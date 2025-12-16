using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Services.DTOs.Movie.Requests
{
    public class BrowseMoviesRequestDto
    {
        public string? Query { get; set; } = string.Empty;
        public int? GenreId { get; set; }
        public double? Rating { get; set; }
        public DateOnly? Date { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
