namespace AiSupport.Domain.Models;

public class Customer
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public AppTenant Tenant { get; set; }
    public string Name { get; set; }
    public string? ExternalId { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime UpdatedDateTime { get; set; }
    public ICollection<PaymentAttempt> PaymentAttempts = new List<PaymentAttempt>();
}
