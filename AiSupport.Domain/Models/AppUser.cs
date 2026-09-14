namespace AiSupport.Domain.Models
{
    public class AppUser
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public IList<AppTenant> GetTenants()
        {
            return new List<AppTenant>();
        }

        public IEnumerable<AppService> GetAppServices()
        {
            return new List<AppService>();
        }

        public Guid AttemtToPayForTenant(Guid tenantId, decimal amount)
        {
            return Guid.NewGuid();
        }

        public Guid AttemtToPayForSertvice(Guid serviceId, decimal amount)
        {
            return Guid.NewGuid();
        }

        public IEnumerable<UserPayment> GetPayments(DateTime fromDate, DateTime toDate)
        {
            return new List<UserPayment>();
        }

        public IEnumerable<UserPayment> GetPaymentsByTenant(Guid tenantId, DateTime fromDate, DateTime toDate)
        {
            return new List<UserPayment>();
        }

        public UserPayment GetLastPayment()
        {
            return new UserPayment();
        }
    }
}
