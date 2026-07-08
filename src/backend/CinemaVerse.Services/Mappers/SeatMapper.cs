using CinemaVerse.Data.Models;
using CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Response;
using CinemaVerse.Services.DTOs.HallSeat.Responses;

namespace CinemaVerse.Services.Mappers
{
    public static class SeatMapper
    {
        public static SeatDetailsDto ToSeatDetailsDto(Seat seat, Hall? hall = null)
        {
            return new SeatDetailsDto
            {
                SeatId = seat.Id,
                SeatLabel = seat.SeatLabel,
                HallId = seat.HallId,
                HallNumber = hall?.HallNumber ?? seat.Hall?.HallNumber ?? string.Empty,
                BranchName = hall?.Branch?.BranchName ?? seat.Hall?.Branch?.BranchName ?? string.Empty
            };
        }

        public static SeatDto ToSeatDto(Seat seat)
        {
            var label = seat.SeatLabel ?? string.Empty;
            return new SeatDto
            {
                SeatId = seat.Id,
                SeatLabel = label,
                SeatRow = label.Length > 0 ? label.Substring(0, 1) : string.Empty,
                SeatColumn = label.Length > 1 ? label.Substring(1) : string.Empty
            };
        }

        /// <summary>
        /// For use when only seat id and label are available (e.g. from BookingSeat).
        /// </summary>
        public static SeatDto ToSeatDto(int seatId, string seatLabel)
        {
            var label = seatLabel ?? string.Empty;
            return new SeatDto
            {
                SeatId = seatId,
                SeatLabel = label,
                SeatRow = label.Length > 0 ? label.Substring(0, 1) : string.Empty,
                SeatColumn = label.Length > 1 ? label.Substring(1) : string.Empty
            };
        }
    }
}
