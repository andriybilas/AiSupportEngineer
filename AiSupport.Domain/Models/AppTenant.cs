namespace AiSupport.Domain.Models
{
    public class AppTenant
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdatedDateTime { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();
    }
}
