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

        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
