using CinemaVerse.Services.DTOs.AdminFlow.AdminBooking.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Responses;
using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminUser.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminUser.Responses;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.Ticket.Response;
using CinemaVerse.Services.Interfaces.Admin;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/users")]
    [ApiController]
    public class AdminUserController : ControllerBase
    {
        private readonly ILogger<AdminUserController> _logger;
        private readonly IAdminUserService _adminUserService;
        private readonly IBookingService _bookingService;
        private readonly IPaymentService _paymentService;
        private readonly ITicketService _ticketService;

        public AdminUserController(IAdminUserService adminUserService, ILogger<AdminUserController> logger, IBookingService bookingService, IPaymentService paymentService, ITicketService ticketService)
        {
            _adminUserService = adminUserService;
            _logger = logger;
            _bookingService = bookingService;
            _paymentService = paymentService;
            _ticketService = ticketService;
        }


        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<UserDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers([FromQuery] AdminUserFilterDto filter)
        {
            if (filter == null)
                filter = new AdminUserFilterDto();
            _logger.LogInformation("Admin: Getting all users, Page {Page}, PageSize {PageSize}", filter.Page, filter.PageSize);
            var result = await _adminUserService.GetAllUsersAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserById(int id)
        {
            _logger.LogInformation("Admin: Getting user with ID: {UserId}", id);
            var user = await _adminUserService.GetUserByIdAsync(id);
            return Ok(user);
        }



        [HttpPost]
        [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
        {
            if (request == null)
                return BadRequest(new { error = new CinemaVerse.Models.ErrorResponse { Message = "Request body is required.", Code = "VALIDATION_ERROR" } });

            _logger.LogInformation("Admin: Creating new user");
            var userId = await _adminUserService.CreateUserAsync(request);
            _logger.LogInformation("Admin: Successfully created user with ID: {UserId}", userId);
            var user = await _adminUserService.GetUserByIdAsync(userId);
            return CreatedAtAction(nameof(GetUserById), new { id = userId }, user);
        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequestDto request)
        {
            if (request == null)
                return BadRequest(new { error = new CinemaVerse.Models.ErrorResponse { Message = "Request body is required.", Code = "VALIDATION_ERROR" } });

            _logger.LogInformation("Admin: Updating user with ID: {UserId}", id);
            await _adminUserService.UpdateUserAsync(id, request);
            _logger.LogInformation("Admin: Successfully updated user with ID: {UserId}", id);
            return NoContent();
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            _logger.LogInformation("Admin: Deleting user with ID: {UserId}", id);
            await _adminUserService.DeleteUserAsync(id);
            _logger.LogInformation("Admin: Successfully deleted user with ID: {UserId}", id);
            return NoContent();
        }


        [HttpPost("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActivateUser(int id)
        {
            _logger.LogInformation("Admin: Activating user with ID: {UserId}", id);
            var result = await _adminUserService.ActivateUserAsync(id);
            return Ok(new { message = "User activated successfully" });
        }

        [HttpPost("{id}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            _logger.LogInformation("Admin: Deactivating user with ID: {UserId}", id);
            var result = await _adminUserService.DeactivateUserAsync(id);
            return Ok(new { message = "User deactivated successfully" });
        }

        [HttpGet("{id}/bookings")]
        [ProducesResponseType(typeof(PagedResultDto<BookingDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserBookings(int id, [FromQuery] AdminBookingFilterDto filter)
        {
            if (filter == null)
                filter = new AdminBookingFilterDto();
            _logger.LogInformation("Admin: Getting bookings for user {UserId}, Page {Page}, PageSize {PageSize}", id, filter.Page, filter.PageSize);
            var result = await _bookingService.GetUserBookingsAsync(id, filter);
            return Ok(result);
        }


        [HttpGet("{id}/tickets")]
        [ProducesResponseType(typeof(PagedResultDto<TicketDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserTickets(int id, [FromQuery] AdminTicketFilterDto filter)
        {
            if (filter == null)
                filter = new AdminTicketFilterDto();
            _logger.LogInformation("Admin: Getting tickets for user {UserId}, Page {Page}, PageSize {PageSize}", id, filter.Page, filter.PageSize);
            var result = await _ticketService.GetUserTicketsAsync(id, filter);
            return Ok(result);
        }

        [HttpGet("{id}/payments")]
        [ProducesResponseType(typeof(PagedResultDto<PaymentDetailsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserPayments(int id, [FromQuery] AdminPaymentFilterDto filter)
        {
            if (filter == null)
                filter = new AdminPaymentFilterDto();
            _logger.LogInformation("Admin: Getting payments for user {UserId}, Page {Page}, PageSize {PageSize}", id, filter.Page, filter.PageSize);
            var result = await _paymentService.GetUserPaymentsAsync(id, filter);
            return Ok(result);
        }

        [HttpGet("export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExportUsers([FromQuery] AdminUserFilterDto filter)
        {
            if (filter == null) filter = new AdminUserFilterDto();
            filter.Page = 1;
            filter.PageSize = 10000; // max export size
            
            _logger.LogInformation("Admin: Exporting users to CSV");
            var result = await _adminUserService.GetAllUsersAsync(filter);
            
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("ID,FullName,Email,PhoneNumber,City,Role,IsActive,IsEmailConfirmed");
            
            foreach (var user in result.Items)
            {
                var fullName = $"{user.FirstName} {user.LastName}";
                builder.AppendLine($"{user.UserId},\"{fullName.Replace("\"", "\"\"")}\",\"{user.Email}\",\"{user.PhoneNumber}\",\"{user.City}\",\"{user.Role}\",{user.IsActive},{user.IsEmailConfirmed}");
            }
            
            var bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            return File(bytes, "text/csv", $"users_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }
    }
}

