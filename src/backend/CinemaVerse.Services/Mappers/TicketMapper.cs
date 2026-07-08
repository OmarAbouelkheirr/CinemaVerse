using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Response;
using CinemaVerse.Services.DTOs.Ticket.Response;

namespace CinemaVerse.Services.Mappers
{
    public static class TicketMapper
    {
        public static TicketDetailsDto ToTicketDetailsDto(Ticket ticket)
        {
            var booking = ticket.Booking;
            var moviePosterUrl = booking?.MovieShowTime?.Movie?.MoviePoster ?? string.Empty;
            return new TicketDetailsDto
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                MovieName = booking?.MovieShowTime?.Movie?.MovieName ?? string.Empty,
                ShowStartTime = booking?.MovieShowTime?.ShowStartTime ?? DateTime.MinValue,
                MovieDuration = booking?.MovieShowTime?.Movie?.MovieDuration ?? TimeSpan.Zero,
                HallNumber = booking?.MovieShowTime?.Hall?.HallNumber ?? string.Empty,
                HallType = booking?.MovieShowTime?.Hall?.HallType ?? default,
                SeatLabel = ticket.Seat?.SeatLabel ?? string.Empty,
                MoviePoster = moviePosterUrl,
                MovieAgeRating = booking?.MovieShowTime?.Movie?.MovieAgeRating ?? default,
                QrToken = ticket.QrToken ?? string.Empty,
                Status = ticket.Status,
                Price = ticket.Price,
                BranchName = booking?.MovieShowTime?.Hall?.Branch?.BranchName ?? string.Empty
            };
        }

        public static TicketListItemDto ToTicketListItemDto(Ticket ticket)
        {
            var booking = ticket.Booking;
            var movieShowTime = booking?.MovieShowTime;
            var movie = movieShowTime?.Movie;
            return new TicketListItemDto
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                MovieName = movie?.MovieName ?? string.Empty,
                ShowStartTime = movieShowTime?.ShowStartTime ?? DateTime.MinValue,
                SeatLabel = ticket.Seat?.SeatLabel ?? string.Empty,
                MoviePoster = movie?.MoviePoster ?? string.Empty,
                Status = ticket.Status,
                Price = ticket.Price
            };
        }

        /// <summary>
        /// Maps to a minimal TicketDetailsDto (for use inside BookingDetailsDto when tickets come from booking.Tickets).
        /// </summary>
        public static TicketDetailsDto ToTicketDetailsDtoForBooking(Ticket ticket)
        {
            return new TicketDetailsDto
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                SeatLabel = ticket.Seat?.SeatLabel ?? string.Empty,
                Price = ticket.Price,
                QrToken = ticket.QrToken ?? string.Empty,
                Status = ticket.Status
            };
        }

        public static AdminTicketDetailsDto ToAdminTicketDetailsDto(Ticket ticket)
        {
            var baseDto = ToTicketDetailsDto(ticket);
            return new AdminTicketDetailsDto
            {
                TicketId = baseDto.TicketId,
                TicketNumber = baseDto.TicketNumber,
                MovieName = baseDto.MovieName,
                ShowStartTime = baseDto.ShowStartTime,
                MovieDuration = baseDto.MovieDuration,
                HallNumber = baseDto.HallNumber,
                HallType = baseDto.HallType,
                SeatLabel = baseDto.SeatLabel,
                MoviePoster = baseDto.MoviePoster,
                MovieAgeRating = baseDto.MovieAgeRating,
                QrToken = baseDto.QrToken,
                Status = baseDto.Status,
                Price = baseDto.Price,
                BranchName = baseDto.BranchName,
                UserId = ticket.Booking?.UserId ?? 0,
                UserEmail = ticket.Booking?.User?.Email ?? string.Empty,
                FullName = ticket.Booking?.User?.FullName ?? string.Empty,
                BookingId = ticket.BookingId,
                BookingStatus = ticket.Booking?.Status ?? BookingStatus.Pending,
                UsedAt = ticket.Status == TicketStatus.Used ? ticket.CreatedAt : null
            };
        }
    }
}
