using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.Email.Requests;
using CinemaVerse.Services.DTOs.UserFlow.Auth;
using CinemaVerse.Services.Interfaces;
using CinemaVerse.Services.Mappers;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserEntity = CinemaVerse.Data.Models.Users.User; // علشان بيحصل مشكله في الاسماء
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace CinemaVerse.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, ILogger<AuthService> logger, IEmailService emailService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailService = emailService;
            _configuration = configuration;
        }


        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
                throw new UnauthorizedAccessException("Invalid credentials.");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60);

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds);

            _logger.LogInformation("User {UserId} with email {Email} logged in successfully", user.Id, user.Email);

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new LoginResponseDto
            {
                AccessToken = tokenString,
                ExpiresAt = expiresAt,
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<int> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException($"User with email '{request.Email}' already exists.");

            var passwordHashed = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new UserEntity
            {
                Email = request.Email,
                PasswordHash = passwordHashed,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                City = request.City,
                DateOfBirth = request.DateOfBirth,
                IsActive = true,
                IsEmailConfirmed = false,
                Gender = request.Gender,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                Role = UserRole.User,
        };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully created user {UserId} with email {Email}", user.Id, user.Email);

            try
            {
                var welcomeEmail = UserMapper.ToWelcomeEmailDto(user);
                await _emailService.SendWelcomeEmailAsync(welcomeEmail);
                _logger.LogInformation("Welcome email sent to {Email} for UserId {UserId}", user.Email, user.Id);
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "Failed to send welcome email to {Email}, but user was created", user.Email);
            }

            return user.Id;
        }
    }
}
