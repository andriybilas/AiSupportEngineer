namespace AiSupport.Domain.Models;

public class PaymentAttempt
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ProviderTransactionId { get; set; }
    public decimal Amount { get; set; }
    public bool IsSuccessful { get; set; }
    public string Currency { get; set; };
    public PaymentStatus Status { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime CreatedDateTime { get; set; }
}
