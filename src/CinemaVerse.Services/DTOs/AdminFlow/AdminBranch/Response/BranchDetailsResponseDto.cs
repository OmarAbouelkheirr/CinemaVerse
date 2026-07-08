using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Response
{
    public class BranchDetailsResponseDto
    {
        public int Id { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string BranchLocation { get; set; } = string.Empty;
        public int TotalHalls { get; set; }
        public int TotalCapacity { get; set; }
        public int TotalShowtimes { get; set; }
    }
}
