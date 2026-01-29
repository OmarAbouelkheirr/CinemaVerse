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
    }
}
