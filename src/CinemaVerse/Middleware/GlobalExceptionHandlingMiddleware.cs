using System.Net;
using System.Text.Json;
using CinemaVerse.Models;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next,ILogger<GlobalExceptionHandlingMiddleware> logger,IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (statusCode, code, message) = MapException(ex);

            if (statusCode == HttpStatusCode.InternalServerError && _env.IsDevelopment())
                message = ex.Message;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorResponse
            {
                Message = message,
                Code = code
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = response }, options));
        }

        private static (HttpStatusCode statusCode, string? code, string message) MapException(Exception ex)
        {
            return ex switch
            {
                KeyNotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND", ex.Message),
                UnauthorizedAccessException => (HttpStatusCode.Forbidden, "FORBIDDEN", ex.Message),
                ArgumentNullException or ArgumentException => (HttpStatusCode.BadRequest, "BAD_REQUEST", ex.Message),
                InvalidOperationException inv when inv.Message.Contains("already", StringComparison.OrdinalIgnoreCase)
                    => (HttpStatusCode.Conflict, "CONFLICT", ex.Message),
                InvalidOperationException => (HttpStatusCode.BadRequest, "BAD_REQUEST", ex.Message),
                _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR",
                    "An error occurred while processing your request.")
            };
        }
    }
}
