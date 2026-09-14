using System;
using System.Collections.Generic;

namespace AiSupport.Infrastructure.DataBase
{
    public class Tenant
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdatedDateTime { get; set; }

        // One-to-many: Tenant -> AppServices
        public virtual ICollection<AppService> AppServices { get; set; } = new List<AppService>();

        // Many-to-many: Tenant <-> AppUser
        public virtual ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();
    }
}
