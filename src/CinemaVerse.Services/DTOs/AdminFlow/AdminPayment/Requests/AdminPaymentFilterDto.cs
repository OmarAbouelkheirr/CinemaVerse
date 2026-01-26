using CinemaVerse.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Requests
{
    public class AdminPaymentFilterDto
    {
        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        
        // Sorting
        public string? SortBy { get; set; } // paymentdate, amount, status
        public string? SortOrder { get; set; } // asc, desc
        
        // Filters
        public int? BookingId { get; set; }
        public int? UserId { get; set; }
        public PaymentStatus? Status { get; set; } // Success, Failed, Refunded, Pending
        public DateTime? PaymentDateFrom { get; set; }
        public DateTime? PaymentDateTo { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? SearchTerm { get; set; } // Search by user email or transaction ID
    }
}
