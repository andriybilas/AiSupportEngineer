namespace AiSupport.Domain.Models;

public class PaymentAttempt
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string ProviderTransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime CreatedDateTime { get; set; }
}
