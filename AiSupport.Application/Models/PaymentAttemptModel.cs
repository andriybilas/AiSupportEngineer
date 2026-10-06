namespace AiSupport.Application.Models
{
    public class PaymentAttemptModel
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string ProviderTransactionId { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? ErrorCode { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
