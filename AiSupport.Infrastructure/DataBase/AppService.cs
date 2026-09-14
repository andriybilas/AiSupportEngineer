using System;
using System.Collections.Generic;

namespace AiSupport.Infrastructure.DataBase
{
    public class AppService
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Description { get; set; }

        // AppService no longer directly references AppUser

        // Tenant relationship (many AppServices belong to one Tenant)
        public Guid? TenantId { get; set; }
        public virtual Tenant? Tenant { get; set; }
    }
}
