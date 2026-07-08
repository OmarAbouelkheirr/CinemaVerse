using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CinemaVerse.Models
{

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? Code { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }

        public static ErrorResponse FromModelState(ModelStateDictionary modelState)
        {
            var errors = modelState
                .Where(ms => ms.Value?.Errors.Count > 0)
                .ToDictionary(
                    ms => ms.Key,
                    ms => ms.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            return new ErrorResponse
            {
                Message = "Validation failed.",
                Code = "VALIDATION_ERROR",
                Errors = errors
            };
        }
    }
}
