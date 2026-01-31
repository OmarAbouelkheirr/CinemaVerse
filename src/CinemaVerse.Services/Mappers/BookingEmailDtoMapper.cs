using CinemaVerse.Data.Models;
using CinemaVerse.Services.DTOs.Email.Flow;
using CinemaVerse.Services.DTOs.Email.Requests;

namespace CinemaVerse.Services.Mappers
{
    public static class BookingEmailDtoMapper
    {
        public static BookingConfirmationEmailDto ToConfirmationEmailDto(Booking booking)
        {
            return new BookingConfirmationEmailDto
            {
                To = booking.User?.Email ?? string.Empty,
                BookingId = booking.Id,
                FullName = booking.User?.FullName ?? string.Empty,
                MovieName = booking.MovieShowTime?.Movie?.MovieName ?? string.Empty,
                ShowStartTime = booking.MovieShowTime?.ShowStartTime ?? DateTime.MinValue,
                ShowEndTime = booking.MovieShowTime?.ShowEndTime ?? DateTime.MinValue,
                HallNumber = booking.MovieShowTime?.Hall?.HallNumber ?? string.Empty,
                BranchName = booking.MovieShowTime?.Hall?.Branch?.BranchName ?? string.Empty,
                TotalAmount = booking.TotalAmount,
                Currency = "EGP",
                Tickets = booking.Tickets?.Select(t => new TicketInfoDto
                {
                    TicketNumber = t.TicketNumber,
                    SeatNumber = t.Seat?.SeatLabel ?? string.Empty,
                    Price = t.Price
                }).ToList() ?? new List<TicketInfoDto>()
            };
        }

        public static BookingReminderEmailDto ToReminderEmailDto(Booking booking)
        {
            return new BookingReminderEmailDto
            {
                To = booking.User?.Email ?? string.Empty,
                BookingId = booking.Id,
                FullName = booking.User?.FullName ?? string.Empty,
                MovieName = booking.MovieShowTime?.Movie?.MovieName ?? string.Empty,
                ShowStartTime = booking.MovieShowTime?.ShowStartTime ?? DateTime.MinValue,
                ShowEndTime = booking.MovieShowTime?.ShowEndTime ?? DateTime.MinValue,
                HallNumber = booking.MovieShowTime?.Hall?.HallNumber ?? string.Empty,
                BranchName = booking.MovieShowTime?.Hall?.Branch?.BranchName ?? string.Empty,
                TotalAmount = booking.TotalAmount,
                Currency = "EGP",
                Tickets = booking.Tickets?.Select(t => new TicketInfoDto
                {
                    TicketNumber = t.TicketNumber,
                    SeatNumber = t.Seat?.SeatLabel ?? string.Empty,
                    Price = t.Price
                }).ToList() ?? new List<TicketInfoDto>()
            };
        }

        public static BookingCancellationEmailDto ToCancellationEmailDto(Booking booking, decimal refundAmount, string cancellationReason)
        {
            return new BookingCancellationEmailDto
            {
                To = booking.User?.Email ?? string.Empty,
                BookingId = booking.Id,
                FullName = booking.User?.FullName ?? string.Empty,
                MovieName = booking.MovieShowTime?.Movie?.MovieName ?? string.Empty,
                ShowStartTime = booking.MovieShowTime?.ShowStartTime ?? DateTime.MinValue,
                RefundAmount = refundAmount,
                Currency = "EGP",
                CancellationReason = cancellationReason
            };
        }

        public static PaymentConfirmationEmailDto ToPaymentConfirmationEmailDto(
            Booking booking,
            string paymentIntentId,
            decimal amount,
            string currency,
            DateTime transactionDate)
        {
            return new PaymentConfirmationEmailDto
            {
                To = booking.User?.Email ?? string.Empty,
                BookingId = booking.Id,
                PaymentIntentId = paymentIntentId,
                Amount = amount,
                Currency = currency,
                TransactionDate = transactionDate,
                MovieName = booking.MovieShowTime?.Movie?.MovieName ?? string.Empty,
                ShowStartTime = booking.MovieShowTime?.ShowStartTime ?? DateTime.MinValue
            };
        }
    }
}
