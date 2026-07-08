using CinemaVerse.Services.Implementations;
using CinemaVerse.Services.Implementations.Admin;
using CinemaVerse.Services.Implementations.Background;
using CinemaVerse.Services.Implementations.User;
using CinemaVerse.Services.Interfaces;
using CinemaVerse.Services.Interfaces.Admin;
using CinemaVerse.Services.Interfaces.Background;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaVerse.Extensions;

public static class ServiceCollectionExtensions
{


    public static IServiceCollection AddUserServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IHallSeatService, HallSeatService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        return services;
    }

    public static IServiceCollection AddAdminServices(this IServiceCollection services)
    {
        services.AddScoped<IAdminBookingService, AdminBookingService>();
        services.AddScoped<IAdminBranchService, AdminBranchService>();
        services.AddScoped<IAdminGenreService, AdminGenreService>();
        services.AddScoped<IAdminHallService, AdminHallService>();
        services.AddScoped<IAdminMovieService, AdminMovieService>();
        services.AddScoped<IAdminPaymentService, AdminPaymentService>();
        services.AddScoped<IAdminSeatService, AdminSeatService>();
        services.AddScoped<IAdminShowtimeService, AdminShowtimeService>();
        services.AddScoped<IAdminTicketService, AdminTicketService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminDashboard, AdminDashboardService>();
        return services;
    }

    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddScoped<IExpirePendingBookingsService, ExpirePendingBookingsService>();
        services.AddScoped<IShowReminderService, ShowReminderService>();
        return services;
    }
}
