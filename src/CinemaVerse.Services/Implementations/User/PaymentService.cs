using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.Email.Requests;
using CinemaVerse.Services.DTOs.UserFlow.Payment.Requests;
using CinemaVerse.Services.DTOs.Payment.Requests;
using CinemaVerse.Services.DTOs.Payment.Response;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stripe;

namespace CinemaVerse.Services.Implementations.User
{
    public class PaymentService : IPaymentService
    {
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _stripeSecretKey;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEmailService _emailService;

        public PaymentService(ILogger<PaymentService> logger, IConfiguration configuration, IUnitOfWork unitOfWork, IServiceProvider serviceProvider, IEmailService emailService)
        {
            _logger = logger;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _stripeSecretKey = _configuration["Stripe:SecretKey"] ?? throw new InvalidOperationException("Stripe API key is not configured in appsettings.json");
            StripeConfiguration.ApiKey = _stripeSecretKey;
            _serviceProvider = serviceProvider;
            _emailService = emailService;
        }
        public async Task<bool> ConfirmPaymentAsync(int userId, ConfirmPaymentRequestDto confirmPaymentDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                _logger.LogInformation("Confirming payment for BookingId: {BookingId} by User: {UserId}", confirmPaymentDto.BookingId, userId);
                if (confirmPaymentDto == null)
                {
                    _logger.LogWarning("ConfirmPaymentRequestDto is null while confirming payment by User: {UserId}", userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new ArgumentNullException(nameof(confirmPaymentDto), "ConfirmPaymentRequestDto cannot be null.");
                }
                if (userId <= 0)
                {
                    _logger.LogWarning("UserId must be greater than zero while confirming payment for BookingId: {BookingId}", confirmPaymentDto.BookingId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new ArgumentException("UserId must be greater than zero.");
                }
                if (confirmPaymentDto.BookingId <= 0)
                {
                    _logger.LogWarning("Invalid BookingId: {BookingId} by User: {UserId}", confirmPaymentDto.BookingId, userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new ArgumentException("BookingId must be greater than zero.");
                }
                var booking = await _unitOfWork.Bookings.GetByIdAsync(confirmPaymentDto.BookingId);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found for BookingId: {BookingId} by User: {UserId}", confirmPaymentDto.BookingId, userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new KeyNotFoundException("Booking not found.");
                }
                if (string.IsNullOrEmpty(confirmPaymentDto.PaymentIntentId))
                {
                    _logger.LogWarning("PaymentIntentId is null or empty for BookingId: {BookingId} by User: {UserId}", confirmPaymentDto.BookingId, userId);
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
                    bp.PaymentIntentId == confirmPaymentDto.PaymentIntentId);
                if (existingPayment == null)
                {
                    _logger.LogWarning("No payment record found for BookingId: {BookingId} with PaymentIntentId: {PaymentIntentId} by User: {UserId}", confirmPaymentDto.BookingId, confirmPaymentDto.PaymentIntentId, userId);
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
                var paymentIntent = await service.GetAsync(confirmPaymentDto.PaymentIntentId);
                if (paymentIntent.Status != "succeeded")
                {
                    _logger.LogWarning("PaymentIntent status is not succeeded for BookingId: {BookingId}. Current Status: {Status} by User: {UserId}", confirmPaymentDto.BookingId, paymentIntent.Status, userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("Payment has not been completed successfully.");
                }
                var amount = paymentIntent.Amount;
                var currency = paymentIntent.Currency;
                if (amount != (long)(existingPayment.Amount * 100))
                {
                    _logger.LogWarning("Amount mismatch for BookingId: {BookingId}. Expected: {ExpectedAmount}, PaymentIntent Amount: {PaymentIntentAmount} by User: {UserId}", confirmPaymentDto.BookingId, booking.TotalAmount * 100, amount, userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("Payment amount does not match the booking total.");
                }
                if (!string.Equals(currency, existingPayment.Currency, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Currency mismatch for BookingId: {BookingId}. Expected: {ExpectedCurrency}, PaymentIntent Currency: {PaymentIntentCurrency} by User: {UserId}", confirmPaymentDto.BookingId, existingPayment.Currency, currency, userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("Payment currency does not match the booking currency.");
                }


                existingPayment.Status = PaymentStatus.Completed;
                await _unitOfWork.BookingPayments.UpdateAsync(existingPayment);
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Payment confirmed and booking updated. BookingId: {BookingId} by User: {UserId}", confirmPaymentDto.BookingId, userId);
                
                // ✅ Resolve BookingService from ServiceProvider to avoid circular dependency
                var bookingService = _serviceProvider.GetRequiredService<IBookingService>();
                await bookingService.ConfirmBookingAsync(userId, booking.Id);

                // Send payment confirmation email (failure must not affect the operation)
                var bookingWithDetails = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(booking.Id);
                if (bookingWithDetails?.User?.IsEmailConfirmed == true)
                {
                    try
                    {
                        var emailRequest = new PaymentConfirmationEmailDto
                        {
                            To = bookingWithDetails.User.Email,
                            BookingId = booking.Id,
                            PaymentIntentId = confirmPaymentDto.PaymentIntentId,
                            Amount = existingPayment.Amount,
                            Currency = existingPayment.Currency ?? "EGP",
                            TransactionDate = DateTime.UtcNow,
                            MovieName = bookingWithDetails.MovieShowTime?.Movie?.MovieName ?? string.Empty,
                            ShowStartTime = bookingWithDetails.MovieShowTime?.ShowStartTime ?? DateTime.MinValue
                        };
                        await _emailService.SendPaymentConfirmationEmailAsync(emailRequest);
                        _logger.LogInformation("Payment confirmation email sent for BookingId {BookingId}", booking.Id);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Failed to send payment confirmation email for BookingId {BookingId}, but payment was confirmed", booking.Id);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while confirming payment for BookingId: {BookingId} by User: {UserId}", confirmPaymentDto.BookingId, userId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<CreatePaymentIntentResponseDto> CreatePaymentIntent(int userId, CreatePaymentIntentRequestDto createPaymentDto)
        {
            int? bookingIdForLog = createPaymentDto?.BookingId;
            try
            {
                if (createPaymentDto == null)
                {
                    _logger.LogWarning("createPaymentDto is null while creating payment intent by User: {UserId}", userId);
                    throw new ArgumentNullException(nameof(createPaymentDto), "CreatePaymentIntentRequestDto cannot be null.");
                }

                _logger.LogInformation("Creating payment intent for BookingId: {BookingId} by User: {UserId}", createPaymentDto.BookingId, userId);

                if (userId <= 0)
                {
                    _logger.LogWarning("UserId must be greater than zero while creating payment intent for BookingId: {BookingId}", createPaymentDto.BookingId);
                    throw new ArgumentException("UserId must be greater than zero.");
                }
                
                if (createPaymentDto.BookingId <= 0)
                {
                    _logger.LogWarning("Invalid BookingId: {BookingId} by User: {UserId}", createPaymentDto.BookingId, userId);
                    throw new ArgumentException("BookingId must be greater than zero.");
                }

                if (createPaymentDto.Amount <= 0)
                {
                    _logger.LogWarning("Invalid amount: {Amount} for BookingId: {BookingId} by User: {UserId}", createPaymentDto.Amount, createPaymentDto.BookingId, userId);
                    throw new ArgumentException("Amount must be greater than zero.");
                }
                if (string.IsNullOrEmpty(createPaymentDto.Currency))
                {
                    _logger.LogWarning("Currency is null or empty for BookingId: {BookingId} by User: {UserId}", createPaymentDto.BookingId, userId);
                    throw new ArgumentException("Currency must be provided.");
                }
                if (string.IsNullOrEmpty(createPaymentDto.PaymentMethodType))
                {
                    createPaymentDto.PaymentMethodType = "card"; // Default to card if not provided
                }

                var booking = await _unitOfWork.Bookings.GetByIdAsync(createPaymentDto.BookingId);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found for BookingId: {BookingId} by User: {UserId}", createPaymentDto.BookingId, userId);
                    throw new KeyNotFoundException("Booking not found.");
                }
                if (booking.UserId != userId)
                {
                    _logger.LogWarning(
                        "Unauthorized payment attempt. RequestUserId: {RequestUserId}, BookingUserId: {BookingUserId}, BookingId: {BookingId}",
                        userId, booking.UserId, booking.Id);
                    throw new UnauthorizedAccessException("You are not authorized to pay for this booking.");
                }
                if (booking.TotalAmount != createPaymentDto.Amount)
                {
                    _logger.LogWarning("Amount mismatch for BookingId: {BookingId}. Expected: {ExpectedAmount}, Provided: {ProvidedAmount} by User: {UserId}", createPaymentDto.BookingId, booking.TotalAmount, createPaymentDto.Amount, userId);
                    // Do NOT trust client-provided amount; always charge the booking total.
                }
                if (booking.Status != BookingStatus.Pending)
                {
                    _logger.LogWarning("Invalid booking status for BookingId: {BookingId}. Current Status: {Status} by User: {UserId}", createPaymentDto.BookingId, booking.Status, userId);
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
                    Currency = createPaymentDto.Currency.ToLowerInvariant(),
                    PaymentMethodTypes = new List<string> { createPaymentDto.PaymentMethodType },
                    Metadata = new Dictionary<string, string>
                    {
                        { "BookingId", createPaymentDto.BookingId.ToString() }
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
                            Currency = createPaymentDto.Currency
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
                    existingPayment.Currency = createPaymentDto.Currency;
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
                        Currency = createPaymentDto.Currency,
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
                    Currency = createPaymentDto.Currency
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating payment intent for BookingId: {BookingId} by User: {UserId}", bookingIdForLog, userId);
                throw;
            }
        }

        // ... AI Code, Omar ...

        public async Task<bool> RefundPaymentAsync(RefundPaymentRequestDto refundPaymentDto)
        {
            // Note: This method does NOT manage its own transaction
            // It should be called within an existing transaction context
            // The calling method (e.g., CancelUserBookingAsync) is responsible for transaction management
            
            try
            {
                _logger.LogInformation("Processing refund for BookingId: {BookingId}, PaymentIntentId: {PaymentIntentId}",
                    refundPaymentDto.BookingId, refundPaymentDto.PaymentIntentId);

                // 1. Validate input
                if (refundPaymentDto == null)
                {
                    _logger.LogWarning("RefundPaymentRequestDto is null");
                    throw new ArgumentNullException(nameof(refundPaymentDto), "RefundPaymentRequestDto cannot be null.");
                }

                if (refundPaymentDto.BookingId <= 0)
                {
                    _logger.LogWarning("Invalid BookingId: {BookingId}", refundPaymentDto.BookingId);
                    throw new ArgumentException("BookingId must be greater than zero.", nameof(refundPaymentDto.BookingId));
                }

                if (string.IsNullOrEmpty(refundPaymentDto.PaymentIntentId))
                {
                    _logger.LogWarning("PaymentIntentId is null or empty for BookingId: {BookingId}", refundPaymentDto.BookingId);
                    throw new ArgumentException("PaymentIntentId cannot be null or empty.", nameof(refundPaymentDto.PaymentIntentId));
                }

                if (refundPaymentDto.RefundAmount <= 0)
                {
                    _logger.LogWarning("Invalid RefundAmount: {RefundAmount} for BookingId: {BookingId}",
                        refundPaymentDto.RefundAmount, refundPaymentDto.BookingId);
                    throw new ArgumentException("RefundAmount must be greater than zero.", nameof(refundPaymentDto.RefundAmount));
                }

                // 2. Get booking payment from database
                var bookingPayment = await _unitOfWork.BookingPayments.FirstOrDefaultAsync(bp =>
                    bp.BookingId == refundPaymentDto.BookingId &&
                    bp.PaymentIntentId == refundPaymentDto.PaymentIntentId);

                if (bookingPayment == null)
                {
                    _logger.LogWarning("No payment record found for BookingId: {BookingId} with PaymentIntentId: {PaymentIntentId}",
                        refundPaymentDto.BookingId, refundPaymentDto.PaymentIntentId);
                    throw new KeyNotFoundException("Payment record not found for the provided BookingId and PaymentIntentId.");
                }

                // 3. Check if payment is already refunded
                if (bookingPayment.Status == PaymentStatus.Cancelled)
                {
                    _logger.LogInformation("Payment already refunded for BookingId: {BookingId}, PaymentIntentId: {PaymentIntentId}",
                        refundPaymentDto.BookingId, refundPaymentDto.PaymentIntentId);
                    return true; // Already refunded - idempotency
                }

                // 4. Check if payment is completed (can only refund completed payments)
                if (bookingPayment.Status != PaymentStatus.Completed)
                {
                    _logger.LogWarning("Cannot refund payment with status {Status} for BookingId: {BookingId}, PaymentIntentId: {PaymentIntentId}",
                        bookingPayment.Status, refundPaymentDto.BookingId, refundPaymentDto.PaymentIntentId);
                    throw new InvalidOperationException($"Cannot refund payment. Only completed payments can be refunded. Current status: {bookingPayment.Status}.");
                }

                // 5. Validate refund amount matches payment amount
                if (bookingPayment.Amount != refundPaymentDto.RefundAmount)
                {
                    _logger.LogWarning("Refund amount mismatch for BookingId: {BookingId}. Expected: {ExpectedAmount}, Provided: {ProvidedAmount}",
                        refundPaymentDto.BookingId, bookingPayment.Amount, refundPaymentDto.RefundAmount);
                    throw new InvalidOperationException($"Refund amount does not match the payment amount. Expected: {bookingPayment.Amount}, Provided: {refundPaymentDto.RefundAmount}.");
                }

                // 6. Get PaymentIntent from Stripe to verify it exists and is succeeded
                var paymentIntentService = new PaymentIntentService();
                PaymentIntent paymentIntent;
                try
                {
                    paymentIntent = await paymentIntentService.GetAsync(refundPaymentDto.PaymentIntentId);
                }
                catch (StripeException ex)
                {
                    _logger.LogError(ex, "Failed to retrieve PaymentIntent {PaymentIntentId} from Stripe for BookingId: {BookingId}",
                        refundPaymentDto.PaymentIntentId, refundPaymentDto.BookingId);
                    throw new InvalidOperationException($"Failed to retrieve payment information from payment processor. Please contact support.");
                }

                if (paymentIntent.Status != "succeeded")
                {
                    _logger.LogWarning("PaymentIntent status is not succeeded for BookingId: {BookingId}. Current Status: {Status}",
                        refundPaymentDto.BookingId, paymentIntent.Status);
                    throw new InvalidOperationException($"Cannot refund payment. Payment status is not succeeded. Current status: {paymentIntent.Status}.");
                }

                // 7. Validate amount and currency match
                var paymentAmountInCents = (long)(bookingPayment.Amount * 100);
                if (paymentIntent.Amount != paymentAmountInCents)
                {
                    _logger.LogWarning("Amount mismatch for BookingId: {BookingId}. Expected: {ExpectedAmount}, PaymentIntent Amount: {PaymentIntentAmount}",
                        refundPaymentDto.BookingId, paymentAmountInCents, paymentIntent.Amount);
                    throw new InvalidOperationException("Payment amount does not match the booking payment amount.");
                }

                if (!string.Equals(paymentIntent.Currency, bookingPayment.Currency, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Currency mismatch for BookingId: {BookingId}. Expected: {ExpectedCurrency}, PaymentIntent Currency: {PaymentIntentCurrency}",
                        refundPaymentDto.BookingId, bookingPayment.Currency, paymentIntent.Currency);
                    throw new InvalidOperationException("Payment currency does not match the booking payment currency.");
                }

                // 8. Create refund in Stripe (CRITICAL: This happens BEFORE database update)
                // If Stripe refund succeeds but database update fails, the refund is already processed
                // This is acceptable because the refund is the desired outcome
                var refundService = new RefundService();
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = refundPaymentDto.PaymentIntentId,
                    Amount = paymentAmountInCents, // Convert to smallest currency unit (cents)
                    Reason = RefundReasons.RequestedByCustomer
                };

                Refund refund;
                try
                {
                    refund = await refundService.CreateAsync(refundOptions);
                    _logger.LogInformation("Refund created in Stripe. RefundId: {RefundId}, Amount: {Amount} {Currency} for BookingId: {BookingId}",
                        refund.Id, refundPaymentDto.RefundAmount, bookingPayment.Currency, refundPaymentDto.BookingId);
                }
                catch (StripeException ex)
                {
                    _logger.LogError(ex, "Failed to create refund in Stripe for BookingId: {BookingId}, PaymentIntentId: {PaymentIntentId}",
                        refundPaymentDto.BookingId, refundPaymentDto.PaymentIntentId);
                    throw new InvalidOperationException($"Failed to process refund. Please contact support. Error: {ex.Message}");
                }

                // 9. Update payment status in database (within existing transaction)
                bookingPayment.Status = PaymentStatus.Cancelled;
                await _unitOfWork.BookingPayments.UpdateAsync(bookingPayment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully processed refund for BookingId: {BookingId}, PaymentIntentId: {PaymentIntentId}, RefundId: {RefundId}",
                    refundPaymentDto.BookingId, refundPaymentDto.PaymentIntentId, refund.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing refund for BookingId: {BookingId}, PaymentIntentId: {PaymentIntentId}",
                    refundPaymentDto?.BookingId, refundPaymentDto?.PaymentIntentId);
                throw;
            }
        }

        public async Task<bool> RefundPaymentForUserAsync(int userId, RefundPaymentRequestDto refundPaymentDto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Processing refund for UserId: {UserId}, BookingId: {BookingId}", userId, refundPaymentDto.BookingId);

                // Validate userId
                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid UserId: {UserId}", userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
                }

                // Validate refundPaymentDto
                if (refundPaymentDto == null)
                {
                    _logger.LogWarning("RefundPaymentDto is null for UserId: {UserId}", userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new ArgumentNullException(nameof(refundPaymentDto), "RefundPaymentRequestDto cannot be null.");
                }

                // Validate that the booking belongs to the user
                var booking = await _unitOfWork.Bookings.GetByIdAsync(refundPaymentDto.BookingId);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found for BookingId: {BookingId} by UserId: {UserId}", refundPaymentDto.BookingId, userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new KeyNotFoundException("Booking not found.");
                }

                if (booking.UserId != userId)
                {
                    _logger.LogWarning("Unauthorized refund attempt. RequestUserId: {RequestUserId}, BookingUserId: {BookingUserId}, BookingId: {BookingId}",
                        userId, booking.UserId, refundPaymentDto.BookingId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new UnauthorizedAccessException("You are not authorized to refund this payment.");
                }

                // Call the existing RefundPaymentAsync method (it doesn't manage transactions)
                var result = await RefundPaymentAsync(refundPaymentDto);

                if (!result)
                {
                    _logger.LogError("Failed to process refund for BookingId {BookingId} for UserId {UserId}", refundPaymentDto.BookingId, userId);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("Failed to process refund.");
                }

                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Successfully processed refund for UserId: {UserId}, BookingId: {BookingId}", userId, refundPaymentDto.BookingId);
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error processing refund for UserId: {UserId}, BookingId: {BookingId}", userId, refundPaymentDto?.BookingId);
                throw;
            }
        }
    }
}
