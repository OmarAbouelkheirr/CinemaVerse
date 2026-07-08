namespace CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Responses
{
    public class PaymentSummaryDto
    {
        public int TotalPayments { get; set; }
        public int CompletedPayments { get; set; }
        public int PendingPayments { get; set; }
        public int FailedPayments { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
