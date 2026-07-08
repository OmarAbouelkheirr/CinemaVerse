using CinemaVerse.Data.Models;
using CinemaVerse.Services.DTOs.HallSeat.Responses;

namespace CinemaVerse.Services.Mappers
{
    public static class HallSeatMapper
    {
        public static HallWithSeatsDto ToHallWithSeatsDto(
            MovieShowTime movieShowTime,
            IEnumerable<Seat> availableSeats,
            IEnumerable<Seat> reservedSeats)
        {
            var hall = movieShowTime.Hall;
            return new HallWithSeatsDto
            {
                HallId = hall?.Id ?? 0,
                HallNumber = hall?.HallNumber ?? string.Empty,
                HallType = hall?.HallType ?? default,
                Branch = hall?.Branch?.BranchLocation ?? string.Empty,
                Capacity = hall?.Capacity ?? 0,
                AvailableSeats = availableSeats.Select(SeatMapper.ToSeatDto).ToList(),
                ReservedSeats = reservedSeats.Select(SeatMapper.ToSeatDto).ToList()
            };
        }
    }
}
