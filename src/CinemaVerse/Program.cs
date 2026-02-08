using System.Text;
using CinemaVerse.Data.Data;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Extensions;
using CinemaVerse.Filters;
using CinemaVerse.Infrastructure;
using CinemaVerse.Middleware;
using CinemaVerse.Services.Constants;
using Microsoft.Extensions.Caching.Memory;
using Hangfire;
using Hangfire.SqlServer;
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

// --- Hangfire (Background Jobs) ---
builder.Services.AddHangfire(configuration =>
{
    configuration
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
        {
            PrepareSchemaIfNecessary = true
        });
});
builder.Services.AddHangfireServer();

// --- Data / Infrastructure ---
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.Configure<CachingOptions>(builder.Configuration.GetSection(CachingOptions.SectionName));
builder.Services.AddMemoryCache();

// --- Application Services (Auth, User, Admin, Background) ---
builder.Services.AddUserServices();
builder.Services.AddAdminServices();
builder.Services.AddBackgroundServices();

builder.Services.AddScoped<ModelStateValidationFilter>();
builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ModelStateValidationFilter>();
    })
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

var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("JWT Secret must be configured and non-empty.");

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
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

// Hangfire Dashboard (consider securing with authorization later)
app.UseHangfireDashboard("/hangfire");

app.MapControllers();

// Configure Hangfire recurring jobs
HangfireJobsConfigurator.ConfigureRecurringJobs();

try
{
    Log.Information("Starting CinemaVerse API");
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}