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
                BranchName = branch.BranchName,
                BranchLocation = branch.BranchLocation
            };
        }
    }
}
