namespace AiSupport.Domain.Models
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public AppTenant? Tenant { get; set; }
        public string Name { get; set; } = string.Empty;
        public SubscriptionStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdatedDateTime { get; set; }
        public ICollection<AppService> AppServices { get; set; } = new List<AppService>();
    }
}
