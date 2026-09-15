using Microsoft.AspNetCore.Identity;

namespace AiSupport.Domain.Models
{
    public class AppUser : IdentityUser<Guid>
    {
        public string? Name { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdatedDateTime { get; set; }
        public ICollection<AppTenant> Tenants { get; set; } = new List<AppTenant>();
    }
}
