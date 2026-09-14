using System;
using Microsoft.AspNetCore.Identity;

namespace AiSupport.Infrastructure.DataBase
{
    public class AppUser : IdentityUser<Guid>
    {
        public string? Name { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdatedDateTime { get; set; }
        public string? Address { get; set; }

        // Many-to-many relationship: AppUser <-> Tenant
        public virtual ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
    }
}
