using System.Text;
using CinemaVerse.Data.Data;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Middleware;
using CinemaVerse.Services.Implementations;
using CinemaVerse.Services.Implementations.Admin;
using CinemaVerse.Services.Implementations.User;
using CinemaVerse.Services.Interfaces;
using CinemaVerse.Services.Interfaces.Admin;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "CinemaVerse.API");
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IHallSeatService, HallSeatService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAdminBookingService, AdminBookingService>();
builder.Services.AddScoped<IAdminBranchService, AdminBranchService>();
builder.Services.AddScoped<IAdminGenreService, AdminGenreService>();
builder.Services.AddScoped<IAdminHallService, AdminHallService>();
builder.Services.AddScoped<IAdminMovieService, AdminMovieService>();
builder.Services.AddScoped<IAdminPaymentService, AdminPaymentService>();
builder.Services.AddScoped<IAdminSeatService, AdminSeatService>();
builder.Services.AddScoped<IAdminShowtimeService, AdminShowtimeService>();
builder.Services.AddScoped<IAdminTicketService, AdminTicketService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAuthService, AuthService>();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter your token from Login (without \"Bearer \" prefix).",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? string.Empty))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("CinemaVerseApiCorsPolicy", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7227",
                "http://localhost:5073"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CinemaVerseApiCorsPolicy");

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting CinemaVerse API");
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}