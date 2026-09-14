namespace AiSupport.Domain.Models
{
    public class AppTenant
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public IList<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
