using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.Payment.NewFolder;
using CinemaVerse.Services.DTOs.Payment.Requests;
using CinemaVerse.Services.DTOs.Payment.Response;
using CinemaVerse.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace CinemaVerse.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _stripeSecretKey;
        private readonly IBookingService _bookingService;

        public PaymentService(ILogger<PaymentService> logger, IConfiguration configuration, IUnitOfWork unitOfWork ,IBookingService bookingService)
        {
            _logger = logger;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _stripeSecretKey = _configuration["Stripe:SecretKey"] ?? throw new InvalidOperationException("Stripe API key is not configured in appsettings.json");
            StripeConfiguration.ApiKey = _stripeSecretKey;
            _bookingService = bookingService;
        }
        public async Task<bool> ConfirmPaymentAsync(int userId, ConfirmPaymentRequestDto ConfrimPaymentDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                _logger.LogInformation("Confirming payment for BookingId: {BookingId} by User: {UserId}", ConfrimPaymentDto.BookingId, userId);
                if (ConfrimPaymentDto == null)
                {
                    _logger.LogWarning("ConfrimPaymentDto is null while confirming payment by User: {UserId}", userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new ArgumentNullException(nameof(ConfrimPaymentDto), "ConfrimPaymentDto cannot be null.");
                }
                if (userId <= 0)
                {
                    _logger.LogWarning("UserId must be greater than zero while confirming payment for BookingId: {BookingId}", ConfrimPaymentDto.BookingId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new ArgumentException("UserId must be greater than zero.");
                }
                if (ConfrimPaymentDto.BookingId <= 0)
                {
                    _logger.LogWarning("Invalid BookingId: {BookingId} by User: {UserId}", ConfrimPaymentDto.BookingId, userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new ArgumentException("BookingId must be greater than zero.");
                }
                var booking = await _unitOfWork.Bookings.GetByIdAsync(ConfrimPaymentDto.BookingId);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found for BookingId: {BookingId} by User: {UserId}", ConfrimPaymentDto.BookingId, userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new KeyNotFoundException("Booking not found.");
                }
                if (string.IsNullOrEmpty(ConfrimPaymentDto.PaymentIntentId))
                {
                    _logger.LogWarning("PaymentIntentId is null or empty for BookingId: {BookingId} by User: {UserId}", ConfrimPaymentDto.BookingId, userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new ArgumentException("PaymentIntentId cannot be null or empty.");
                }
                if (booking.UserId != userId)
                {
                    _logger.LogWarning(
                        "Unauthorized payment confirmation attempt. RequestUserId: {RequestUserId}, BookingUserId: {BookingUserId}, BookingId: {BookingId}",
                        userId, booking.UserId, booking.Id);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new UnauthorizedAccessException("You are not authorized to confirm payment for this booking.");
                }
                var existingPayment = await _unitOfWork.BookingPayments.FirstOrDefaultAsync(bp =>
                    bp.BookingId == booking.Id &&
                    bp.PaymentIntentId == ConfrimPaymentDto.PaymentIntentId);
                if (existingPayment == null)
                {
                    _logger.LogWarning("No payment record found for BookingId: {BookingId} with PaymentIntentId: {PaymentIntentId} by User: {UserId}", ConfrimPaymentDto.BookingId, ConfrimPaymentDto.PaymentIntentId, userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new KeyNotFoundException("Payment record not found for the provided BookingId and PaymentIntentId.");
                }
                // ✅ Check if already confirmed (idempotency)
                if (existingPayment.Status == PaymentStatus.Completed)
                {
                    _logger.LogInformation("Payment already confirmed for BookingId: {BookingId}", booking.Id);
                    await _unitOfWork.RollbackTransactionAsync(); // No changes needed
                    return true; // Already confirmed - safe to return true
                }

                if (booking.Status == BookingStatus.Confirmed)
                {
                    _logger.LogInformation("Booking already confirmed. BookingId: {BookingId}", booking.Id);
                    await _unitOfWork.RollbackTransactionAsync();
                    return true;
                }

                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(ConfrimPaymentDto.PaymentIntentId);
                if (paymentIntent.Status != "succeeded")
                {
                    _logger.LogWarning("PaymentIntent status is not succeeded for BookingId: {BookingId}. Current Status: {Status} by User: {UserId}", ConfrimPaymentDto.BookingId, paymentIntent.Status, userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("Payment has not been completed successfully.");
                }
                var amount = paymentIntent.Amount;
                var currency = paymentIntent.Currency;
                if (amount != (long)(existingPayment.Amount * 100))
                {
                    _logger.LogWarning("Amount mismatch for BookingId: {BookingId}. Expected: {ExpectedAmount}, PaymentIntent Amount: {PaymentIntentAmount} by User: {UserId}", ConfrimPaymentDto.BookingId, booking.TotalAmount * 100, amount, userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("Payment amount does not match the booking total.");
                }
                if (!string.Equals(currency, existingPayment.Currency, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Currency mismatch for BookingId: {BookingId}. Expected: {ExpectedCurrency}, PaymentIntent Currency: {PaymentIntentCurrency} by User: {UserId}", ConfrimPaymentDto.BookingId, existingPayment.Currency, currency, userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("Payment currency does not match the booking currency.");
                }


                existingPayment.Status = PaymentStatus.Completed;
                await _unitOfWork.BookingPayments.UpdateAsync(existingPayment);
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Payment confirmed and booking updated. BookingId: {BookingId} by User: {UserId}", ConfrimPaymentDto.BookingId, userId);
                await _bookingService.ConfirmBookingAsync(userId,booking.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while confirming payment for BookingId: {BookingId} by User: {UserId}", ConfrimPaymentDto.BookingId, userId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<CreatePaymentIntentResponseDto> CreatePaymentIntent(int userId, CreatePaymentIntentRequestDto CreatePaymentDto)
        {
            int? bookingIdForLog = CreatePaymentDto?.BookingId;
            try
            {
                if (CreatePaymentDto == null)
                {
                    _logger.LogWarning("CreatePaymentDto is null while creating payment intent by User: {UserId}", userId);
                    throw new ArgumentNullException(nameof(CreatePaymentDto), "CreatePaymentDto cannot be null.");
                }

                _logger.LogInformation("Creating payment intent for BookingId: {BookingId} by User: {UserId}", CreatePaymentDto.BookingId, userId);

                if (userId <= 0)
                {
                    _logger.LogWarning("UserId must be greater than zero while creating payment intent for BookingId: {BookingId}", CreatePaymentDto.BookingId);
                    throw new ArgumentException("UserId must be greater than zero.");
                }
                
                if (CreatePaymentDto.BookingId <= 0)
                {
                    _logger.LogWarning("Invalid BookingId: {BookingId} by User: {UserId}", CreatePaymentDto.BookingId, userId);
                    throw new ArgumentException("BookingId must be greater than zero.");
                }

                if (CreatePaymentDto.Amount <= 0)
                {
                    _logger.LogWarning("Invalid amount: {Amount} for BookingId: {BookingId} by User: {UserId}", CreatePaymentDto.Amount, CreatePaymentDto.BookingId, userId);
                    throw new ArgumentException("Amount must be greater than zero.");
                }
                if (string.IsNullOrEmpty(CreatePaymentDto.Currency))
                {
                    _logger.LogWarning("Currency is null or empty for BookingId: {BookingId} by User: {UserId}", CreatePaymentDto.BookingId, userId);
                    throw new ArgumentException("Currency must be provided.");
                }
                if (string.IsNullOrEmpty(CreatePaymentDto.PaymentMethodType))
                {
                    CreatePaymentDto.PaymentMethodType = "card"; // Default to card if not provided
                }

                var booking = await _unitOfWork.Bookings.GetByIdAsync(CreatePaymentDto.BookingId);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found for BookingId: {BookingId} by User: {UserId}", CreatePaymentDto.BookingId, userId);
                    throw new KeyNotFoundException("Booking not found.");
                }
                if (booking.UserId != userId)
                {
                    _logger.LogWarning(
                        "Unauthorized payment attempt. RequestUserId: {RequestUserId}, BookingUserId: {BookingUserId}, BookingId: {BookingId}",
                        userId, booking.UserId, booking.Id);
                    throw new UnauthorizedAccessException("You are not authorized to pay for this booking.");
                }
                if (booking.TotalAmount != CreatePaymentDto.Amount)
                {
                    _logger.LogWarning("Amount mismatch for BookingId: {BookingId}. Expected: {ExpectedAmount}, Provided: {ProvidedAmount} by User: {UserId}", CreatePaymentDto.BookingId, booking.TotalAmount, CreatePaymentDto.Amount, userId);
                    // Do NOT trust client-provided amount; always charge the booking total.
                }
                if (booking.Status != BookingStatus.Pending)
                {
                    _logger.LogWarning("Invalid booking status for BookingId: {BookingId}. Current Status: {Status} by User: {UserId}", CreatePaymentDto.BookingId, booking.Status, userId);
                    throw new InvalidOperationException("Booking is not in a valid state for payment.");
                }
                if (booking.ExpiresAt.HasValue && booking.ExpiresAt.Value <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Booking has expired. BookingId: {BookingId}, ExpiresAt: {ExpiresAt}",
                        booking.Id, booking.ExpiresAt.Value);

                    // Optionally: Update booking status to Expired
                    booking.Status = BookingStatus.Expired;
                    await _unitOfWork.Bookings.UpdateAsync(booking);
                    await _unitOfWork.SaveChangesAsync();

                    throw new InvalidOperationException("This booking has expired. Please create a new booking.");
                }

                // Idempotency: if there's already a pending PaymentIntent for this booking, reuse it.
                var existingPayment = await _unitOfWork.BookingPayments.FirstOrDefaultAsync(bp =>
                    bp.BookingId == booking.Id &&
                    bp.Status == PaymentStatus.Pending &&
                    bp.PaymentIntentId != "");

                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(booking.TotalAmount * 100), // Convert to smallest currency unit
                    Currency = CreatePaymentDto.Currency.ToLowerInvariant(),
                    PaymentMethodTypes = new List<string> { CreatePaymentDto.PaymentMethodType },
                    Metadata = new Dictionary<string, string>
                    {
                        { "BookingId", CreatePaymentDto.BookingId.ToString() }
                    }
                };
                var service = new PaymentIntentService();

                if (existingPayment != null)
                {
                    try
                    {
                        var existingIntent = await service.GetAsync(existingPayment.PaymentIntentId);
                        _logger.LogInformation("Reusing existing payment intent. BookingId: {BookingId}, PaymentIntentId: {PaymentIntentId}",
                            booking.Id, existingIntent.Id);

                        return new CreatePaymentIntentResponseDto
                        {
                            PaymentIntentId = existingIntent.Id,
                            ClientSecret = existingIntent.ClientSecret,
                            Amount = booking.TotalAmount,
                            Currency = CreatePaymentDto.Currency
                        };
                    }
                    catch (StripeException ex)
                    {
                        _logger.LogWarning(ex,
                            "Failed to fetch existing PaymentIntent {PaymentIntentId} for BookingId {BookingId}. Creating a new one.",
                            existingPayment.PaymentIntentId, booking.Id);
                        // Fall through to create a new PaymentIntent and update the pending record.
                    }
                }

                PaymentIntent paymentIntent = await service.CreateAsync(options);

                // Persist PaymentIntentId for later confirmation/refund workflows
                if (existingPayment != null)
                {
                    existingPayment.Amount = booking.TotalAmount;
                    existingPayment.Currency = CreatePaymentDto.Currency;
                    existingPayment.PaymentIntentId = paymentIntent.Id;
                    existingPayment.TransactionDate = DateTime.UtcNow;
                    existingPayment.Status = PaymentStatus.Pending;
                    await _unitOfWork.BookingPayments.UpdateAsync(existingPayment);
                }
                else
                {
                    var bookingPayment = new BookingPayment
                    {
                        BookingId = booking.Id,
                        Amount = booking.TotalAmount,
                        Currency = CreatePaymentDto.Currency,
                        PaymentIntentId = paymentIntent.Id,
                        TransactionDate = DateTime.UtcNow,
                        Status = PaymentStatus.Pending
                    };
                    await _unitOfWork.BookingPayments.AddAsync(bookingPayment);
                }
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Payment intent created and saved. BookingId: {BookingId}, PaymentIntentId: {PaymentIntentId}",
                    booking.Id, paymentIntent.Id);

                return new CreatePaymentIntentResponseDto
                {
                    PaymentIntentId = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    Amount = booking.TotalAmount,
                    Currency = CreatePaymentDto.Currency
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating payment intent for BookingId: {BookingId} by User: {UserId}", bookingIdForLog, userId);
                throw;
            }
        }

        public async Task<bool> RefundPaymentAsync(RefundPaymentRequestDto RefundPaymentDto)
        {
            throw new NotImplementedException();
        }
    }
}
