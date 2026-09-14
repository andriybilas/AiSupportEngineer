using AiSupport.Domain.Models;

namespace AiSupport.Application.Models
{
    public class CreateSubscriptionRequest
    {
        public Guid TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Guid> AppServiceIds { get; set; } = new List<Guid>();
    }
}
