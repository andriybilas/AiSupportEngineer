namespace AiSupport.Domain.Models;

public class PaymentAttempt
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; }
    public string ProviderTransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; };
    public PaymentStatus Status { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime CreatedDateTime { get; set; }
}
