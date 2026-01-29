using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.Constants;
using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Responses;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Services.Implementations.Admin
{
    public class AdminPaymentService : IAdminPaymentService
    {
        private readonly ILogger<AdminPaymentService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public AdminPaymentService(ILogger<AdminPaymentService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResultDto<PaymentDetailsResponseDto>> GetAllPaymentsAsync(AdminPaymentFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting all payments with filter: {@Filter}", filter);

                if (filter == null)
                {
                    _logger.LogWarning("AdminPaymentFilterDto is null.");
                    throw new ArgumentNullException(nameof(filter), "AdminPaymentFilterDto cannot be null.");
                }

                // Validate pagination
                if (filter.Page <= 0)
                    filter.Page = 1;

                if (filter.PageSize <= 0 || filter.PageSize > PaginationConstants.MaxPageSize)
                    filter.PageSize = PaginationConstants.DefaultPageSize;

                // Build query
                var query = _unitOfWork.BookingPayments.GetQueryable();

                // Apply filters
                if (filter.BookingId.HasValue)
                {
                    query = query.Where(p => p.BookingId == filter.BookingId.Value);
                }

                if (filter.UserId.HasValue)
                {
                    query = query.Where(p => p.Booking.UserId == filter.UserId.Value);
                }

                if (filter.Status.HasValue)
                {
                    query = query.Where(p => p.Status == filter.Status.Value);
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

                // Search by user email, transaction ID, or reference number
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchLower = filter.SearchTerm.ToLower();
                    query = query.Where(p =>
                        p.Booking.User.Email.ToLower().Contains(searchLower));
                }

                // Get total count before pagination
                var totalCount = await _unitOfWork.BookingPayments.CountAsync(query);

                // Apply sorting
                string sortBy = filter.SortBy?.ToLower() ?? "transactiondate";
                string sortOrder = filter.SortOrder?.ToLower() ?? "desc";

                if (sortBy == "transactiondate")
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
                else // Default
                {
                    query = query.OrderByDescending(p => p.TransactionDate);
                }

                // Get paged results with related entities
                var payments = await _unitOfWork.BookingPayments.GetPagedAsync(
                    query: query,
                    orderBy: null,
                    skip: (filter.Page - 1) * filter.PageSize,
                    take: filter.PageSize,
                    includeProperties: "Booking.User,Booking.MovieShowTime.Movie"
                );

                // Map to DTOs
                var paymentDtos = payments.Select(payment => new PaymentDetailsResponseDto
                {
                    PaymentId = payment.Id,
                    BookingId = payment.BookingId,
                    Amount = payment.Amount,
                    TransactionDate = payment.TransactionDate,
                    Status = payment.Status,
                    Currency = payment.Currency
                }).ToList();

                var pagedResult = new PagedResultDto<PaymentDetailsResponseDto>
                {
                    Items = paymentDtos,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };

                _logger.LogInformation("Retrieved {Count} payments out of {Total} total",
                    paymentDtos.Count, totalCount);

                return pagedResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all payments.");
                throw;
            }
        }

        public async Task<PaymentDetailsResponseDto?> GetPaymentByIdAsync(int paymentId)
        {
            try
            {
                _logger.LogInformation("Getting payment with Id {PaymentId}", paymentId);

                if (paymentId <= 0)
                {
                    _logger.LogWarning("Invalid PaymentId {PaymentId} provided for retrieval", paymentId);
                    throw new ArgumentException("A valid PaymentId must be provided.", nameof(paymentId));
                }

                // Get payment with related entities
                var payment = await _unitOfWork.BookingPayments.GetByIdAsync(paymentId);

                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found", paymentId);
                    return null;
                }

                var paymentDto = new PaymentDetailsResponseDto
                {
                    PaymentId = payment.Id,
                    BookingId = payment.BookingId,
                    Amount = payment.Amount,
                    TransactionDate = payment.TransactionDate,
                    Status = payment.Status,
                    Currency = payment.Currency

                };

                _logger.LogInformation("Successfully retrieved payment {PaymentId}", paymentId);
                return paymentDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting payment with Id {PaymentId}", paymentId);
                throw;
            }
        }
    }
}
