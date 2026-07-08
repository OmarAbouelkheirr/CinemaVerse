using CinemaVerse.Data.Models;
using CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Response;

namespace CinemaVerse.Services.Mappers
{
    public static class BranchMapper
    {
        public static BranchDetailsResponseDto ToDetailsResponseDto(Branch branch)
        {
            return new BranchDetailsResponseDto
            {
                Id = branch.Id,
                BranchName = branch.BranchName,
                BranchLocation = branch.BranchLocation,
                TotalHalls = branch.Halls?.Count ?? 0,
                TotalCapacity = branch.Halls?.Sum(h => h.Capacity) ?? 0,
                TotalShowtimes = branch.Halls?.Sum(h => h.MovieShowTimes?.Count ?? 0) ?? 0
            };
        }
    }
}
