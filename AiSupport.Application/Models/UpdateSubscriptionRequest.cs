using AiSupport.Domain.Models;

namespace AiSupport.Application.Models
{
    public class UpdateSubscriptionRequest
    {
        public string? Name { get; set; }
        public SubscriptionStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<Guid>? AppServiceIds { get; set; }
    }
}
