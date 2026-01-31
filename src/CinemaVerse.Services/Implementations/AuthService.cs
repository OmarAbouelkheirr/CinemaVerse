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
        private const string EmailVerificationPurpose = "email_verification";
        private const string PasswordResetPurpose = "password_reset";
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

        private string GenerateEmailVerificationToken(int userId, string email)
        {
            var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
            var issuer = _configuration["Jwt:Issuer"];
            var expirationHours = _configuration.GetValue<int>("EmailVerification:ExpirationHours", 24);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim("purpose", EmailVerificationPurpose)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddHours(expirationHours);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: "CinemaVerseEmailVerification",
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private (int UserId, string Email)? ValidateEmailVerificationToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            var secret = _configuration["Jwt:Secret"];
            if (string.IsNullOrEmpty(secret)) return null;

            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));

            try
            {
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = "CinemaVerseEmailVerification",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var purpose = principal.FindFirst("purpose")?.Value;
                if (purpose != EmailVerificationPurpose) return null;

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId)) return null;

                return (userId, emailClaim ?? string.Empty);
            }
            catch
            {
                return null;
            }
        }

        private string GeneratePasswordResetToken(int userId, string email)
        {
            var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
            var expirationHours = _configuration.GetValue<int>("PasswordReset:ExpirationHours", 1);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim("purpose", PasswordResetPurpose)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddHours(expirationHours);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: "CinemaVersePasswordReset",
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private (int UserId, string Email)? ValidatePasswordResetToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            var secret = _configuration["Jwt:Secret"];
            if (string.IsNullOrEmpty(secret)) return null;

            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));

            try
            {
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = "CinemaVersePasswordReset",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                var purpose = principal.FindFirst("purpose")?.Value;
                if (purpose != PasswordResetPurpose) return null;

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId)) return null;

                return (userId, emailClaim ?? string.Empty);
            }
            catch
            {
                return null;
            }
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
                throw new UnauthorizedAccessException("Invalid credentials.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Invalid credentials.");

            var requireEmailVerification = _configuration.GetValue<bool>("Auth:RequireEmailVerificationBeforeLogin", false);
            if (requireEmailVerification && !user.IsEmailConfirmed)
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
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (await _unitOfWork.Users.IsEmailExistsAsync(request.Email))
                throw new InvalidOperationException("An account with this email already exists.");

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

            try
            {
                var verificationToken = GenerateEmailVerificationToken(user.Id, user.Email);
                var baseUrl = _configuration["EmailVerification:VerificationLinkBaseUrl"]?.TrimEnd('/') ?? "https://localhost:7001";
                var verificationLink = $"{baseUrl}/api/auth/verify-email?token={Uri.EscapeDataString(verificationToken)}";
                var expirationHours = _configuration.GetValue<int>("EmailVerification:ExpirationHours", 24);
                var verificationEmail = new EmailVerificationEmailDto
                {
                    To = user.Email,
                    FullName = user.FullName,
                    Subject = "Confirm Your Email - CinemaVerse",
                    VerificationLink = verificationLink,
                    ExpirationTime = DateTime.UtcNow.AddHours(expirationHours)
                };
                await _emailService.SendEmailVerificationEmailAsync(verificationEmail);
                _logger.LogInformation("Verification email sent to {Email} for UserId {UserId}", user.Email, user.Id);
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "Failed to send verification email to {Email}, but user was created", user.Email);
            }

            return user.Id;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var payload = ValidateEmailVerificationToken(token);
            if (payload == null)
            {
                _logger.LogWarning("Invalid or expired email verification token");
                return false;
            }

            var user = await _unitOfWork.Users.GetByIdAsync(payload.Value.UserId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for email verification", payload.Value.UserId);
                return false;
            }

            if (user.IsEmailConfirmed)
            {
                _logger.LogInformation("User {UserId} email already confirmed", user.Id);
                return true;
            }

            user.IsEmailConfirmed = true;
            user.LastUpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Email verified for UserId {UserId}", user.Id);
            return true;
        }

        public async Task<bool> ResendEmailVerificationAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Resend verification: email is empty");
                return false;
            }

            var user = await _unitOfWork.Users.GetByEmailAsync(email.Trim());
            if (user == null)
            {
                _logger.LogWarning("Resend verification: user not found for email {Email}", email);
                return false;
            }

            if (user.IsEmailConfirmed)
            {
                _logger.LogInformation("Resend verification: email already confirmed for {Email}", email);
                return true;
            }

            try
            {
                var verificationToken = GenerateEmailVerificationToken(user.Id, user.Email);
                var baseUrl = _configuration["EmailVerification:VerificationLinkBaseUrl"]?.TrimEnd('/') ?? "https://localhost:7001";
                var verificationLink = $"{baseUrl}/api/auth/verify-email?token={Uri.EscapeDataString(verificationToken)}";
                var expirationHours = _configuration.GetValue<int>("EmailVerification:ExpirationHours", 24);
                var verificationEmail = new EmailVerificationEmailDto
                {
                    To = user.Email,
                    FullName = user.FullName,
                    Subject = "Confirm Your Email - CinemaVerse",
                    VerificationLink = verificationLink,
                    ExpirationTime = DateTime.UtcNow.AddHours(expirationHours)
                };
                await _emailService.SendEmailVerificationEmailAsync(verificationEmail);
                _logger.LogInformation("Verification email resent to {Email} for UserId {UserId}", user.Email, user.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend verification email to {Email}", user.Email);
                return false;
            }
        }

        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("RequestPasswordReset: email is empty");
                return false;
            }

            var user = await _unitOfWork.Users.GetByEmailAsync(email.Trim());
            if (user == null)
            {
                _logger.LogWarning("RequestPasswordReset: user not found for email {Email}", email);
                return true;
            }

            try
            {
                var resetToken = GeneratePasswordResetToken(user.Id, user.Email);
                var baseUrl = _configuration["PasswordReset:ResetLinkBaseUrl"]?.TrimEnd('/')
                    ?? _configuration["EmailVerification:VerificationLinkBaseUrl"]?.TrimEnd('/')
                    ?? "https://localhost:7227";
                var resetPagePath = _configuration["PasswordReset:ResetPagePath"]?.TrimStart('/').TrimEnd('/');
                var path = string.IsNullOrEmpty(resetPagePath) ? "reset-password" : resetPagePath.TrimStart('/');
                var resetLink = $"{baseUrl}/{path}?token={Uri.EscapeDataString(resetToken)}";
                var expirationHours = _configuration.GetValue<int>("PasswordReset:ExpirationHours", 1);
                var resetEmail = new PasswordResetEmailDto
                {
                    To = user.Email,
                    Subject = "Reset Your Password - CinemaVerse",
                    ResetToken = resetToken,
                    ResetLink = resetLink,
                    ExpirationTime = DateTime.UtcNow.AddHours(expirationHours)
                };
                await _emailService.SendPasswordResetEmailAsync(resetEmail);
                _logger.LogInformation("Password reset email sent to {Email} for UserId {UserId}", user.Email, user.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var payload = ValidatePasswordResetToken(token);
            if (payload == null)
            {
                _logger.LogWarning("Invalid or expired password reset token");
                return false;
            }

            var user = await _unitOfWork.Users.GetByIdAsync(payload.Value.UserId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for password reset", payload.Value.UserId);
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.LastUpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Password reset completed for UserId {UserId}", user.Id);
            return true;
        }
    }
}
