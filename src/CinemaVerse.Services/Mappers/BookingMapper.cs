using CinemaVerse.Data.Models;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.HallSeat.Responses;
using CinemaVerse.Services.DTOs.Ticket.Response;
using CinemaVerse.Services.DTOs.UserFlow.Booking.Helpers;

namespace CinemaVerse.Services.Mappers
{
    public static class BookingMapper
    {
        public static BookingDetailsDto ToDetailsDto(Booking booking, IEnumerable<TicketDetailsDto>? tickets = null)
        {
            var posterUrl = booking.MovieShowTime?.Movie?.MoviePoster ?? string.Empty;

            var bookedSeats = booking.BookingSeats?
                .Select(bs =>
                {
                    var label = bs.Seat?.SeatLabel ?? string.Empty;
                    return new SeatDto
                    {
                        SeatId = bs.SeatId,
                        SeatLabel = label,
                        SeatRow = label.Length > 0 ? label.Substring(0, 1) : string.Empty,
                        SeatColumn = label.Length > 1 ? label.Substring(1) : string.Empty
                    };
                })
                .ToList() ?? new List<SeatDto>();

            var ticketList = tickets?.ToList()
                ?? booking.Tickets?.Select(t => TicketMapper.ToTicketDetailsDtoForBooking(t)).ToList()
                ?? new List<TicketDetailsDto>();

            return new BookingDetailsDto
            {
                BookingId = booking.Id,
                Status = booking.Status,
                TotalAmount = booking.TotalAmount,
                CreatedAt = booking.CreatedAt,
                ExpiresAt = booking.ExpiresAt,
                CustomerName = booking.User?.FullName,
                Showtime = new ShowtimeDto
                {
                    MovieShowTimeId = booking.MovieShowTime?.Id ?? 0,
                    MovieTitle = booking.MovieShowTime?.Movie?.MovieName ?? string.Empty,
                    StartTime = booking.MovieShowTime?.ShowStartTime ?? DateTime.MinValue,
                    PosterUrl = posterUrl
                },
                BookedSeats = bookedSeats,
                Tickets = ticketList
            };
        }

        public static BookingListDto ToListDto(Booking booking)
        {
            return new BookingListDto
            {
                BookingId = booking.Id,
                Status = booking.Status,
                TotalAmount = booking.TotalAmount,
                CreatedAt = booking.CreatedAt,
                MovieShowTimeId = booking.MovieShowTime?.Id ?? 0,
                ShowStartTime = booking.MovieShowTime?.ShowStartTime ?? DateTime.MinValue,
                MovieTitle = booking.MovieShowTime?.Movie?.MovieName ?? string.Empty,
                TicketsCount = booking.Tickets?.Count ?? 0
            };
        }

    }
}
