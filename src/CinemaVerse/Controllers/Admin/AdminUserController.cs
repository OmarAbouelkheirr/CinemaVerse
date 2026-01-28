using CinemaVerse.Services.DTOs.AdminFlow.AdminBooking.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Responses;
using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminUser.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminUser.Responses;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.Ticket.Response;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.Admin
{
    [Route("api/admin/users")]
    [ApiController]
    public class AdminUserController : ControllerBase
    {
        private readonly ILogger<AdminUserController> _logger;
        private readonly IAdminUserService _adminUserService;

        public AdminUserController(
            IAdminUserService adminUserService,
            ILogger<AdminUserController> logger)
        {
            _adminUserService = adminUserService;
            _logger = logger;
        }


        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<UserDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers([FromQuery] AdminUserFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Admin: Getting all users with filter: {@Filter}", filter);
                
                var result = await _adminUserService.GetAllUsersAsync(filter);
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new { error = "An error occurred while retrieving users" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                _logger.LogInformation("Admin: Getting user with ID: {UserId}", id);
                
                if (id <= 0)
                {
                    return BadRequest(new { error = "User ID must be greater than zero" });
                }

                var user = await _adminUserService.GetUserByIdAsync(id);
                
                if (user == null)
                {
                    return NotFound(new { error = $"User with ID {id} not found" });
                }

                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID: {UserId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the user" });
            }
        }

        [HttpGet("email/{email}")]
        [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            try
            {
                _logger.LogInformation("Admin: Getting user with email: {Email}", email);
                
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(new { error = "Email cannot be null or empty" });
                }

                var user = await _adminUserService.GetUserByEmailAsync(email);
                
                if (user == null)
                {
                    return NotFound(new { error = $"User with email {email} not found" });
                }

                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid email: {Email}", email);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with email: {Email}", email);
                return StatusCode(500, new { error = "An error occurred while retrieving the user" });
            }
        }


        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
        {
            try
            {
                _logger.LogInformation("Admin: Creating new user: {Email}", request?.Email);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = await _adminUserService.CreateUserAsync(request!);
                
                _logger.LogInformation("Admin: Successfully created user with ID: {UserId}", userId);
                
                return CreatedAtAction(
                    nameof(GetUserById),
                    new { id = userId },
                    new { id = userId, message = "User created successfully" });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null request received");
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request data");
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation");
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { error = "An error occurred while creating the user" });
            }
        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequestDto request)
        {
            try
            {
                _logger.LogInformation("Admin: Updating user with ID: {UserId}", id);
                
                if (id <= 0)
                {
                    return BadRequest(new { error = "User ID must be greater than zero" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _adminUserService.UpdateUserAsync(id, request);
                
                _logger.LogInformation("Admin: Successfully updated user with ID: {UserId}", id);
                
                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null request received");
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request data for user ID: {UserId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", id);
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation for user ID: {UserId}", id);
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the user" });
            }
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                _logger.LogInformation("Admin: Deleting user with ID: {UserId}", id);
                
                if (id <= 0)
                {
                    return BadRequest(new { error = "User ID must be greater than zero" });
                }

                await _adminUserService.DeleteUserAsync(id);
                
                _logger.LogInformation("Admin: Successfully deleted user with ID: {UserId}", id);
                
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID: {UserId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", id);
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete user with ID: {UserId}", id);
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the user" });
            }
        }


        [HttpPost("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActivateUser(int id)
        {
            try
            {
                _logger.LogInformation("Admin: Activating user with ID: {UserId}", id);
                
                if (id <= 0)
                {
                    return BadRequest(new { error = "User ID must be greater than zero" });
                }

                var result = await _adminUserService.ActivateUserAsync(id);
                
                if (!result)
                {
                    return NotFound(new { error = $"User with ID {id} not found" });
                }

                return Ok(new { message = "User activated successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID: {UserId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user with ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while activating the user" });
            }
        }

        [HttpPost("{id}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            try
            {
                _logger.LogInformation("Admin: Deactivating user with ID: {UserId}", id);
                
                if (id <= 0)
                {
                    return BadRequest(new { error = "User ID must be greater than zero" });
                }

                var result = await _adminUserService.DeactivateUserAsync(id);
                
                if (!result)
                {
                    return NotFound(new { error = $"User with ID {id} not found" });
                }

                return Ok(new { message = "User deactivated successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID: {UserId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user with ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while deactivating the user" });
            }
        }


        [HttpPost("{id}/confirm-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmUserEmail(int id)
        {
            try
            {
                _logger.LogInformation("Admin: Confirming email for user with ID: {UserId}", id);
                
                if (id <= 0)
                {
                    return BadRequest(new { error = "User ID must be greater than zero" });
                }

                var result = await _adminUserService.ConfirmUserEmailAsync(id);
                
                if (!result)
                {
                    return NotFound(new { error = $"User with ID {id} not found" });
                }

                return Ok(new { message = "User email confirmed successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID: {UserId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email for user with ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while confirming the user email" });
            }
        }

        [HttpPost("{id}/unconfirm-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UnconfirmUserEmail(int id)
        {
            try
            {
                _logger.LogInformation("Admin: Unconfirming email for user with ID: {UserId}", id);
                
                if (id <= 0)
                {
                    return BadRequest(new { error = "User ID must be greater than zero" });
                }

                var result = await _adminUserService.UnconfirmUserEmailAsync(id);
                
                if (!result)
                {
                    return NotFound(new { error = $"User with ID {id} not found" });
                }

                return Ok(new { message = "User email unconfirmed successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid user ID: {UserId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unconfirming email for user with ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while unconfirming the user email" });
            }
        }

        [HttpGet("{id}/bookings")]
        [ProducesResponseType(typeof(PagedResultDto<BookingDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserBookings(int id, [FromQuery] AdminBookingFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Admin: Getting bookings for user {UserId} with filter: {@Filter}", id, filter);
                
                if (id <= 0)
                {
                    return BadRequest(new { error = "User ID must be greater than zero" });
                }

                var result = await _adminUserService.GetUserBookingsAsync(id, filter);
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters for user ID: {UserId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings for user {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving user bookings" });
            }
        }


        [HttpGet("{id}/tickets")]
        [ProducesResponseType(typeof(PagedResultDto<TicketDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserTickets(int id, [FromQuery] AdminTicketFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Admin: Getting tickets for user {UserId} with filter: {@Filter}", id, filter);
                
                if (id <= 0)
                {
                    return BadRequest(new { error = "User ID must be greater than zero" });
                }

                var result = await _adminUserService.GetUserTicketsAsync(id, filter);
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters for user ID: {UserId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets for user {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving user tickets" });
            }
        }

        [HttpGet("{id}/payments")]
        [ProducesResponseType(typeof(PagedResultDto<PaymentDetailsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserPayments(int id, [FromQuery] AdminPaymentFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Admin: Getting payments for user {UserId} with filter: {@Filter}", id, filter);
                
                if (id <= 0)
                {
                    return BadRequest(new { error = "User ID must be greater than zero" });
                }

                var result = await _adminUserService.GetUserPaymentsAsync(id, filter);
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters for user ID: {UserId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found: {UserId}", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for user {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving user payments" });
            }
        }
    }
}
