using CinemaVerse.Data.Models;
using CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Response;
using CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Response;

namespace CinemaVerse.Services.Mappers
{
    public static class HallMapper
    {
        public static HallDetailsResponseDto ToHallDetailsResponseDto(Hall hall)
        {
            return new HallDetailsResponseDto
            {
                BranchId = hall.BranchId,
                Capacity = hall.Capacity,
                HallNumber = hall.HallNumber,
                HallStatus = hall.HallStatus.ToString(),
                HallType = hall.HallType.ToString()
            };
        }

        public static HallDetailsResponseDto ToHallDetailsResponseDtoWithSeats(Hall hall)
        {
            var dto = ToHallDetailsResponseDto(hall);
            dto.Seats = hall.Seats?
                .Select(s => SeatMapper.ToSeatDetailsDto(s, hall))
                .ToList() ?? new List<SeatDetailsDto>();
            return dto;
        }
    }
}
