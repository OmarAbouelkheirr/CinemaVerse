using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Models.Users;
using CinemaVerse.Data.Repositories;
using UserEntity = CinemaVerse.Data.Models.Users.User;
using CinemaVerse.Services.Constants;
using CinemaVerse.Services.DTOs.AdminFlow.AdminBooking.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Responses;
using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminUser.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminUser.Responses;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.Email.Requests;
using CinemaVerse.Services.DTOs.Ticket.Response;
using CinemaVerse.Services.Interfaces.Admin;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using CinemaVerse.Services.DTOs.UserFlow.Booking.Helpers;
using CinemaVerse.Services.Mappers;

namespace CinemaVerse.Services.Implementations.Admin
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminUserService> _logger;
        private readonly IEmailService _emailService;

        public AdminUserService(IUnitOfWork unitOfWork, ILogger<AdminUserService> logger, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<int> CreateUserAsync(CreateUserRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Admin: Creating new user");

                // Check if email already exists (business rule)
                var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
                if (existingUser != null)
                    throw new InvalidOperationException($"User with email '{request.Email}' already exists.");

                // Hash password
                var passwordHash = HashPassword(request.Password);

                var user = new UserEntity
                {
                    Email = request.Email.Trim().ToLower(),
                    PasswordHash = passwordHash,
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    PhoneNumber = request.PhoneNumber?.Trim(),
                    Address = request.Address.Trim(),
                    City = request.City.Trim(),
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    IsActive = true, // Default to active when created by admin
                    IsEmailConfirmed = false, // Admin can confirm later
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully created user {UserId}", user.Id);

                try
                {
                    var welcomeEmail = new WelcomeEmailDto
                    {
                        To = user.Email,
                        FullName = user.FullName,
                        Subject = "Welcome to CinemaVerse"
                    };
                    await _emailService.SendWelcomeEmailAsync(welcomeEmail);
                    _logger.LogInformation("Welcome email sent for UserId {UserId}", user.Id);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send welcome email for UserId {UserId}, but user was created", user.Id);
                }

                return user.Id;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating user");
                throw;
            }
        }

        public async Task<int> UpdateUserAsync(int userId, UpdateUserRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Updating user with ID: {UserId}", userId);

                if (userId <= 0)
                    throw new ArgumentException("User ID must be a positive integer.", nameof(userId));

                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found.");

                // Check for email conflict if being changed
                if (!string.IsNullOrWhiteSpace(request.Email) && request.Email.ToLower() != user.Email.ToLower())
                {
                    var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
                    if (existingUser != null && existingUser.Id != userId)
                        throw new InvalidOperationException($"User with email '{request.Email}' already exists.");
                }

                // Update properties (PATCH style - only update if provided)
                if (!string.IsNullOrWhiteSpace(request.Email))
                    user.Email = request.Email.Trim().ToLower();

                if (!string.IsNullOrWhiteSpace(request.FirstName))
                    user.FirstName = request.FirstName.Trim();

                if (!string.IsNullOrWhiteSpace(request.LastName))
                    user.LastName = request.LastName.Trim();

                if (request.PhoneNumber != null)
                    user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

                if (!string.IsNullOrWhiteSpace(request.Address))
                    user.Address = request.Address.Trim();

                if (!string.IsNullOrWhiteSpace(request.City))
                    user.City = request.City.Trim();

                if (request.DateOfBirth.HasValue)
                    user.DateOfBirth = request.DateOfBirth.Value;

                if (request.Gender.HasValue)
                    user.Gender = request.Gender.Value;

                if (request.IsActive.HasValue)
                    user.IsActive = request.IsActive.Value;

                if (request.IsEmailConfirmed.HasValue)
                    user.IsEmailConfirmed = request.IsEmailConfirmed.Value;

                if (request.Role.HasValue)
                    user.Role = request.Role.Value;

                user.LastUpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully updated user {UserId}", userId);
                return userId;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating user with ID: {UserId}", userId);
                throw;
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Deleting user with ID: {UserId}", userId);

                if (userId <= 0)
                    throw new ArgumentException("User ID must be a positive integer.", nameof(userId));

                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found.");

                // Check if user has bookings
                var hasBookings = await _unitOfWork.Bookings
                    .AnyAsync(b => b.UserId == userId);

                if (hasBookings)
                    throw new InvalidOperationException(
                        $"Cannot delete user {userId} because they have associated bookings. Please deactivate the user instead.");

                await _unitOfWork.Users.DeleteAsync(user);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully deleted user {UserId}", userId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<UserDetailsDto?> GetUserByIdAsync(int userId)
        {
            _logger.LogInformation("Getting user with ID: {UserId}", userId);

            if (userId <= 0)
                throw new ArgumentException("User ID must be a positive integer.", nameof(userId));

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            var dto = UserMapper.ToUserDetailsDto(user);
            _logger.LogInformation("Successfully retrieved user {UserId}: {Email}", userId, user.Email);
            return dto;
        }

        public async Task<UserDetailsDto?> GetUserByEmailAsync(string email)
        {
            _logger.LogInformation("Getting user with email: {Email}", email);

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));

            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null)
                throw new KeyNotFoundException($"User with email '{email}' not found.");

            var dto = UserMapper.ToUserDetailsDto(user);
            _logger.LogInformation("Successfully retrieved user with email {Email}", email);
            return dto;
        }

        public async Task<PagedResultDto<UserDetailsDto>> GetAllUsersAsync(AdminUserFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting all users with filter: {@Filter}", filter);

                // Validate pagination
                if (filter.Page <= 0)
                    filter.Page = 1;

                if (filter.PageSize <= 0 || filter.PageSize > PaginationConstants.MaxPageSize)
                    filter.PageSize = PaginationConstants.DefaultPageSize;

                // Build query
                var query = _unitOfWork.Users.GetQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchLower = filter.SearchTerm.ToLower();
                    query = query.Where(u =>
                        u.Email.ToLower().Contains(searchLower) ||
                        u.FirstName.ToLower().Contains(searchLower) ||
                        u.LastName.ToLower().Contains(searchLower));
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == filter.IsActive.Value);
                }

                if (filter.IsEmailConfirmed.HasValue)
                {
                    query = query.Where(u => u.IsEmailConfirmed == filter.IsEmailConfirmed.Value);
                }

                if (filter.Gender.HasValue)
                {
                    query = query.Where(u => u.Gender == filter.Gender.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.City))
                {
                    query = query.Where(u => u.City.ToLower().Contains(filter.City.ToLower()));
                }

                if (filter.CreatedFrom.HasValue)
                {
                    query = query.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);
                }

                if (filter.CreatedTo.HasValue)
                {
                    query = query.Where(u => u.CreatedAt <= filter.CreatedTo.Value);
                }

                if (filter.DateOfBirthFrom.HasValue)
                {
                    query = query.Where(u => u.DateOfBirth >= filter.DateOfBirthFrom.Value);
                }

                if (filter.DateOfBirthTo.HasValue)
                {
                    query = query.Where(u => u.DateOfBirth <= filter.DateOfBirthTo.Value);
                }

                // Get total count before pagination
                var totalCount = await _unitOfWork.Users.CountAsync(query);

                // Build orderBy function
                string sortBy = filter.SortBy?.ToLower() ?? "createdat";
                string sortOrder = filter.SortOrder?.ToLower() ?? "desc";

                Func<IQueryable<UserEntity>, IOrderedQueryable<UserEntity>> orderByFunc = sortBy switch
                {
                    "email" => sortOrder == "asc"
                        ? q => q.OrderBy(u => u.Email)
                        : q => q.OrderByDescending(u => u.Email),
                    "fullname" => sortOrder == "asc"
                        ? q => q.OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                        : q => q.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName),
                    "lastupdatedat" => sortOrder == "asc"
                        ? q => q.OrderBy(u => u.LastUpdatedAt)
                        : q => q.OrderByDescending(u => u.LastUpdatedAt),
                    _ => sortOrder == "asc"
                        ? q => q.OrderBy(u => u.CreatedAt)
                        : q => q.OrderByDescending(u => u.CreatedAt)
                };

                // Apply pagination
                var users = await _unitOfWork.Users.GetPagedAsync(
                    query: query,
                    orderBy: orderByFunc,
                    skip: (filter.Page - 1) * filter.PageSize,
                    take: filter.PageSize,
                    includeProperties: null
                );

                // Map to DTOs
                var userDtos = users.Select(user => UserMapper.ToUserDetailsDto(user)).ToList();

                _logger.LogInformation("Retrieved {Count} users out of {Total} total",
                    userDtos.Count, totalCount);

                return new PagedResultDto<UserDetailsDto>
                {
                    Items = userDtos,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw;
            }
        }

        public async Task<bool> ActivateUserAsync(int userId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Activating user with ID: {UserId}", userId);

                if (userId <= 0)
                    throw new ArgumentException("User ID must be a positive integer.", nameof(userId));
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                if (user.IsActive)
                {
                    _logger.LogInformation("User with ID {UserId} is already active", userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return true;
                }

                user.IsActive = true;
                user.LastUpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully activated user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error activating user with ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Deactivating user with ID: {UserId}", userId);

                if (userId <= 0)
                    throw new ArgumentException("User ID must be a positive integer.", nameof(userId));
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                if (!user.IsActive)
                {
                    _logger.LogInformation("User with ID {UserId} is already inactive", userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return true;
                }

                user.IsActive = false;
                user.LastUpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully deactivated user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error deactivating user with ID: {UserId}", userId);
                throw;
            }
        }



        public async Task<PagedResultDto<BookingDetailsDto>> GetUserBookingsAsync(int userId, AdminBookingFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting bookings for user {UserId} with filter: {@Filter}", userId, filter);

                if (userId <= 0)
                    throw new ArgumentException("User ID must be a positive integer.", nameof(userId));
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found.");

                // Validate pagination
                if (filter.Page <= 0)
                    filter.Page = 1;

                if (filter.PageSize <= 0 || filter.PageSize > PaginationConstants.MaxPageSize)
                    filter.PageSize = PaginationConstants.DefaultPageSize;

                // Build query - filter by userId
                var query = _unitOfWork.Bookings.GetQueryable()
                    .Where(b => b.UserId == userId);

                // Apply additional filters
                if (filter.Status.HasValue)
                {
                    query = query.Where(b => b.Status == filter.Status.Value);
                }

                if (filter.MovieShowTimeId.HasValue)
                {
                    query = query.Where(b => b.MovieShowTimeId == filter.MovieShowTimeId.Value);
                }

                if (filter.CreatedFrom.HasValue)
                {
                    query = query.Where(b => b.CreatedAt >= filter.CreatedFrom.Value);
                }

                if (filter.CreatedTo.HasValue)
                {
                    query = query.Where(b => b.CreatedAt <= filter.CreatedTo.Value);
                }

                if (filter.MinAmount.HasValue)
                {
                    query = query.Where(b => b.TotalAmount >= filter.MinAmount.Value);
                }

                if (filter.MaxAmount.HasValue)
                {
                    query = query.Where(b => b.TotalAmount <= filter.MaxAmount.Value);
                }

                // Get total count
                var totalCount = await _unitOfWork.Bookings.CountAsync(query);

                // Apply sorting
                string sortBy = filter.SortBy?.ToLower() ?? "createdat";
                string sortOrder = filter.SortOrder?.ToLower() ?? "desc";

                if (sortBy == "createdat")
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(b => b.CreatedAt)
                        : query.OrderByDescending(b => b.CreatedAt);
                }
                else if (sortBy == "totalamount")
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(b => b.TotalAmount)
                        : query.OrderByDescending(b => b.TotalAmount);
                }
                else if (sortBy == "status")
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(b => b.Status)
                        : query.OrderByDescending(b => b.Status);
                }
                else
                {
                    query = query.OrderByDescending(b => b.CreatedAt);
                }

                // Get paged results with related entities
                var bookings = await _unitOfWork.Bookings.GetPagedAsync(
                    query: query,
                    orderBy: null,
                    skip: (filter.Page - 1) * filter.PageSize,
                    take: filter.PageSize,
                    includeProperties: "User,MovieShowTime.Movie.MovieImages,BookingSeats.Seat,Tickets"
                );

                // Map to DTOs
                var bookingDtos = bookings.Select(b => BookingMapper.ToDetailsDto(b)).ToList();

                _logger.LogInformation("Retrieved {Count} bookings for user {UserId} out of {Total} total",
                    bookingDtos.Count, userId, totalCount);

                return new PagedResultDto<BookingDetailsDto>
                {
                    Items = bookingDtos,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for user {UserId}", userId);
                throw;
            }
        }


        public async Task<PagedResultDto<PaymentDetailsResponseDto>> GetUserPaymentsAsync(int userId, AdminPaymentFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting payments for user {UserId} with filter: {@Filter}", userId, filter);

                if (userId <= 0)
                    throw new ArgumentException("User ID must be a positive integer.", nameof(userId));
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found.");

                // Validate pagination
                if (filter.Page <= 0)
                    filter.Page = 1;

                if (filter.PageSize <= 0 || filter.PageSize > PaginationConstants.MaxPageSize)
                    filter.PageSize = PaginationConstants.DefaultPageSize;

                // Build query - get payments through bookings
                var query = _unitOfWork.BookingPayments.GetQueryable()
                    .Where(p => p.Booking.UserId == userId);

                // Apply filters
                if (filter.Status.HasValue)
                {
                    query = query.Where(p => p.Status == filter.Status.Value);
                }

                if (filter.BookingId.HasValue)
                {
                    query = query.Where(p => p.BookingId == filter.BookingId.Value);
                }

                if (filter.PaymentDateFrom.HasValue)
                {
                    query = query.Where(p => p.TransactionDate >= filter.PaymentDateFrom.Value);
                }

                if (filter.PaymentDateTo.HasValue)
                {
                    query = query.Where(p => p.TransactionDate <= filter.PaymentDateTo.Value);
                }

                if (filter.MinAmount.HasValue)
                {
                    query = query.Where(p => p.Amount >= filter.MinAmount.Value);
                }

                if (filter.MaxAmount.HasValue)
                {
                    query = query.Where(p => p.Amount <= filter.MaxAmount.Value);
                }

                // Get total count
                var totalCount = await _unitOfWork.BookingPayments.CountAsync(query);

                // Apply sorting
                string sortBy = filter.SortBy?.ToLower() ?? "paymentdate";
                string sortOrder = filter.SortOrder?.ToLower() ?? "desc";

                if (sortBy == "paymentdate")
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(p => p.TransactionDate)
                        : query.OrderByDescending(p => p.TransactionDate);
                }
                else if (sortBy == "amount")
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(p => p.Amount)
                        : query.OrderByDescending(p => p.Amount);
                }
                else if (sortBy == "status")
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(p => p.Status)
                        : query.OrderByDescending(p => p.Status);
                }
                else
                {
                    query = query.OrderByDescending(p => p.TransactionDate);
                }

                // Get paged results
                var payments = await _unitOfWork.BookingPayments.GetPagedAsync(
                    query: query,
                    orderBy: null,
                    skip: (filter.Page - 1) * filter.PageSize,
                    take: filter.PageSize,
                    includeProperties: "Booking"
                );

                // Map to DTOs
                var paymentDtos = payments.Select(PaymentMapper.ToPaymentDetailsResponseDto).ToList();

                _logger.LogInformation("Retrieved {Count} payments for user {UserId} out of {Total} total",
                    paymentDtos.Count, userId, totalCount);

                return new PagedResultDto<PaymentDetailsResponseDto>
                {
                    Items = paymentDtos,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for user {UserId}", userId);
                throw;
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
