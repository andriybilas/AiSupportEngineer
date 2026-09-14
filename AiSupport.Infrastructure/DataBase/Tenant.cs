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

        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

        public virtual ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();
    }
}
