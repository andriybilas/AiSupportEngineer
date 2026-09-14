namespace AiSupport.Application.Models
{
    public class AppUserModel
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public IEnumerable<AppTenantModel> Tenants { get; set; } = Enumerable.Empty<AppTenantModel>();
    }
}
