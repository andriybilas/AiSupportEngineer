using System;
using System.Collections.Generic;

namespace AiSupport.Infrastructure.DataBase
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public virtual Tenant Tenant { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime UpdatedDateTime { get; set; }

        public virtual ICollection<AppService> AppServices { get; set; } = new List<AppService>();
    }
}
