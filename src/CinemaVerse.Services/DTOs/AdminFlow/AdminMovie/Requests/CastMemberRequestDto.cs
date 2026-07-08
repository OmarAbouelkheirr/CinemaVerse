using CinemaVerse.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminMovie.Requests
{
    public class CastMemberRequestDto
    {
        [Required(ErrorMessage = "Person name is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Person name must be between 1 and 200 characters")]
        public string PersonName { get; set; } = string.Empty;

        [Url(ErrorMessage = "Image URL must be a valid URL")]
        [StringLength(500, ErrorMessage = "Image URL must not exceed 500 characters")]
        public string? ImageUrl { get; set; }

        [EnumDataType(typeof(CastRoleType), ErrorMessage = "Invalid role type")]
        public CastRoleType RoleType { get; set; }

        [StringLength(200, ErrorMessage = "Character name must not exceed 200 characters")]
        public string? CharacterName { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsLead { get; set; }
    }
}
