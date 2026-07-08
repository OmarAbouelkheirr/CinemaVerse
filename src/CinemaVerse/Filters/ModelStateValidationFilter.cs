using CinemaVerse.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CinemaVerse.Filters;

public class ModelStateValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
            return;

        var errorResponse = ErrorResponse.FromModelState(context.ModelState);
        context.Result = new BadRequestObjectResult(new { error = errorResponse });
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
