using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Requests
{
    public class CreateBranchRequestDto
    {
        [Required(ErrorMessage ="Branch name is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Branch name must be between 1 and 200 characters")]
        public string BranchName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Branch Location is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Branch Location must be between 1 and 500 characters")]
        public string BranchLocation { get; set; } = string.Empty;
    }
}
