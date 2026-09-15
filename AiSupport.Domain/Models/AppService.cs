namespace AiSupport.Domain.Models
{
    public class AppService
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
