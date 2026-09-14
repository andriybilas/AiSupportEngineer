namespace AiSupport.Application.Models
{
    public class SubscriptionModel
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Name { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdatedDateTime { get; set; }
        public IEnumerable<AppServiceModel> AppServices { get; set; } = Enumerable.Empty<AppServiceModel>();
    }
}
