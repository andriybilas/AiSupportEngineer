using AiSupport.Domain.Models;

namespace AiSupport.Application.Abstractions
{
    public interface ITenantRepository
    {
        Task<IEnumerable<AppTenant>> GetUserTenantsAsync(Guid userId);
    }
}
