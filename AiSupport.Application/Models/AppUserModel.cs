namespace AiSupport.Application.Models
{
    public class AppUserModel
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public AppTenantModel? Tenant { get; set; }

        /// <summary>
        /// App services the user can access: distinct services of the tenant's Active subscriptions.
        /// </summary>
        public IEnumerable<AppServiceModel> Services { get; set; } = Enumerable.Empty<AppServiceModel>();
    }
}
