using AiSupport.Application.Models;
using AiSupport.Domain.Models;

namespace AiSupport.Application.Abstractions
{
    public interface ITenantRepository
    {
        Task<IEnumerable<AppTenant>> GetUserTenantsAsync(Guid userId);

        Task<AppTenant?> GetByIdAsync(Guid id);

        Task<IEnumerable<AppTenant>> GetAllAsync();

        Task<AppTenant> CreateAsync(string name, string description);

        Task<EntityOperationResult> UpdateAsync(Guid id, string? name, string? description);

        Task<EntityOperationResult> DeleteAsync(Guid id);

        Task<EntityOperationResult> AddUserAsync(Guid tenantId, Guid userId);

        Task<EntityOperationResult> RemoveUserAsync(Guid tenantId, Guid userId);
    }
}
