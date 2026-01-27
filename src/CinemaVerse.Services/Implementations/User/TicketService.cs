using System.Security.Cryptography;
using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.Ticket.Response;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.Extensions.Logging;


namespace CinemaVerse.Services.Implementations.User
{
    public class TicketService : ITicketService
    {
        private readonly ILogger<TicketService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public TicketService(IUnitOfWork UnitOfWork, ILogger<TicketService> Logger)
        {
            _unitOfWork = UnitOfWork;
            _logger = Logger;
        }

        private static string GenerateSecureQrToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes);
        }

        public async Task<IEnumerable<TicketDetailsDto>> IssueTicketsAsync(int BookingId)
        {
            try
            {
                //Input Validation
                _logger.LogInformation("Issuing ticket for Booking ID: {BookingId}", BookingId);
                if (BookingId <= 0)
                {
                    _logger.LogWarning("Invalid Booking ID: {BookingId}", BookingId);
                    throw new ArgumentException("Invalid Booking ID.");
                }

                var Booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(BookingId);

                if (Booking is null)
                {
                    _logger.LogWarning("Booking ID: {BookingId} does not exist", BookingId);
                    throw new KeyNotFoundException("Booking not found.");
                }
                if (Booking.Status != BookingStatus.Confirmed)
                {
                    _logger.LogWarning("Booking ID: {BookingId} is not confirmed. Current Status: {Status}", BookingId, Booking.Status);
                    throw new InvalidOperationException("Booking is not confirmed.");
                }

                // for idempotency check - avoid re-issuing tickets for already issued seats
                var issuedSeatIds = await _unitOfWork.Tickets.GetIssuedSeatIdsAsync(BookingId);
                var seatsToIssue = Booking.BookingSeats.Where(bs => !issuedSeatIds.Contains(bs.SeatId)).ToList();
                if (!seatsToIssue.Any())
                {
                    _logger.LogInformation(
                        "All tickets already issued for Booking ID: {BookingId}", BookingId);

                    return Enumerable.Empty<TicketDetailsDto>();
                }

                // Logic to issue ticket goes here
                await _unitOfWork.BeginTransactionAsync();
                var CreatedTickets = new List<Ticket>();
                foreach (var seat in seatsToIssue)
                {
                    var Ticket = new Ticket
                    {

                        TicketNumber = GenerateTicketNumber(),
                        BookingId = Booking.Id,
                        SeatId = seat.SeatId,
                        Price = Booking.MovieShowTime.Price,
                        CreatedAt = DateTime.UtcNow,
                        Status = TicketStatus.Active,
                        QrToken = GenerateSecureQrToken()
                    };
                    await _unitOfWork.Tickets.AddAsync(Ticket);
                    CreatedTickets.Add(Ticket);
                }
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully issued {TicketCount} tickets for Booking ID: {BookingId}", CreatedTickets.Count, BookingId);

                var TicketDtos = new List<TicketDetailsDto>();
                foreach (var ticket in CreatedTickets)
                {
                    TicketDtos.Add(MapToDto(ticket));
                }
                await _unitOfWork.CommitTransactionAsync();
                return TicketDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error issuing tickets for Booking ID: {BookingId}", BookingId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        private string GenerateTicketNumber()
        {
            _logger.LogInformation("Generating Ticket Number ");
            string Prefix = "tk-";
            string DatePart = DateTime.UtcNow.ToString("yyyyMMdd");
            string uniquePart = Guid.NewGuid().ToString("N")[..6].ToUpper();
            return $"{Prefix}-{DatePart}-{uniquePart}";
        }
        public TicketDetailsDto MapToDto(Ticket Ticket)
        {
            var Booking = Ticket.Booking;
            var MoviePoster = Booking.MovieShowTime.Movie.MovieImages.FirstOrDefault();
            return new TicketDetailsDto
            {
                TicketId = Ticket.Id,
                TicketNumber = Ticket.TicketNumber,
                MovieName = Booking.MovieShowTime.Movie.MovieName,
                ShowStartTime = Booking.MovieShowTime.ShowStartTime,
                MovieDuration = Booking.MovieShowTime.Movie.MovieDuration,
                HallNumber = Booking.MovieShowTime.Hall.HallNumber,
                HallType = Booking.MovieShowTime.Hall.HallType,
                SeatLabel = Ticket.Seat.SeatLabel,
                MoviePoster = MoviePoster?.ImageUrl ?? string.Empty,
                MovieAgeRating = Booking.MovieShowTime.Movie.MovieAgeRating,
                QrToken = Ticket.QrToken,
                Status = Ticket.Status,
                Price = Ticket.Price,
                BranchName = Booking.MovieShowTime.Hall.Branch.BranchName
            };
        }
    }
}
