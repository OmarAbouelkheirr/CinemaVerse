using CinemaVerse.Data.Models;
using CinemaVerse.Services.DTOs.AdminFlow.AdminShowtime.Response;
using CinemaVerse.Services.DTOs.UserFlow.Booking.Helpers;
using CinemaVerse.Services.DTOs.UserFlow.Movie.Flow;

namespace CinemaVerse.Services.Mappers
{
    public static class ShowtimeMapper
    {
        /// <summary>
        /// Maps to the simple ShowtimeDto used in BookingDetailsDto (MovieShowTimeId, StartTime, MovieTitle, PosterUrl).
        /// </summary>
        public static ShowtimeDto ToShowtimeDto(MovieShowTime movieShowTime)
        {
            return new ShowtimeDto
            {
                MovieShowTimeId = movieShowTime.Id,
                StartTime = movieShowTime.ShowStartTime,
                MovieTitle = movieShowTime.Movie?.MovieName ?? string.Empty,
                PosterUrl = movieShowTime.Movie?.MoviePoster ?? string.Empty
            };
        }

        /// <summary>
        /// Maps to the full MovieShowTimeDto used in MovieDetailsDto.
        /// </summary>
        public static MovieShowTimeDto ToMovieShowTimeDto(MovieShowTime movieShowTime)
        {
            return new MovieShowTimeDto
            {
                MovieShowTimeId = movieShowTime.Id,
                ShowStartTime = movieShowTime.ShowStartTime,
                HallId = movieShowTime.HallId,
                HallNumber = movieShowTime.Hall?.HallNumber ?? string.Empty,
                HallType = movieShowTime.Hall?.HallType ?? default,
                BranchId = movieShowTime.Hall?.BranchId ?? 0,
                BranchName = movieShowTime.Hall?.Branch?.BranchName ?? string.Empty,
                BranchLocation = movieShowTime.Hall?.Branch?.BranchLocation ?? string.Empty,
                TicketPrice = movieShowTime.Price
            };
        }

        public static ShowtimeDetailsDto ToShowtimeDetailsDto(MovieShowTime movieShowTime)
        {
            return new ShowtimeDetailsDto
            {
                Id = movieShowTime.Id,
                MovieId = movieShowTime.MovieId,
                MovieName = movieShowTime.Movie?.MovieName ?? string.Empty,
                HallId = movieShowTime.HallId,
                HallNumber = movieShowTime.Hall?.HallNumber ?? string.Empty,
                BranchId = movieShowTime.Hall?.BranchId ?? 0,
                BranchName = movieShowTime.Hall?.Branch?.BranchName ?? string.Empty,
                ShowStartTime = movieShowTime.ShowStartTime,
                ShowEndTime = movieShowTime.ShowEndTime,
                Price = movieShowTime.Price,
                CreatedAt = movieShowTime.CreatedAt,
                TotalBookings = movieShowTime.Bookings?.Count ?? 0,
                TotalTickets = movieShowTime.Bookings?.SelectMany(b => b.Tickets ?? Enumerable.Empty<Ticket>()).Count() ?? 0
            };
        }
    }
}
