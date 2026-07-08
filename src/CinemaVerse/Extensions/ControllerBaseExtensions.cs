using System.Security.Claims;
using CinemaVerse.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CinemaVerse.Extensions
{

    public static class ControllerBaseExtensions
    {
        public static IActionResult BadRequestFromValidation(this ControllerBase controller, ModelStateDictionary modelState)
        {
            return controller.BadRequest(new { error = ErrorResponse.FromModelState(modelState) });
        }

        public static int? GetCurrentUserId(this ControllerBase controller)
        {
            var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var parsedUserId))
                return null;
            return parsedUserId;
        }
    }
}
