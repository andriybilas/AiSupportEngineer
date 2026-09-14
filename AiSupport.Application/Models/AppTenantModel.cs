namespace AiSupport.Application.Models
{
    public class AppTenantModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdatedDateTime { get; set; }
        public IEnumerable<SubscriptionModel> Subscriptions { get; set; } = Enumerable.Empty<SubscriptionModel>();
    }
}
