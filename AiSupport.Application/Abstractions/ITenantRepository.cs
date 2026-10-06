using AiSupport.Application.Models;
using AiSupport.Domain.Models;

namespace AiSupport.Application.Abstractions
{
    public interface ITenantRepository
    {

        Task<AppTenant?> GetByIdAsync(Guid id);

        Task<IEnumerable<AppTenant>> GetAllAsync();

        Task<AppTenant> CreateAsync(string name, string description);

        Task<EntityOperationResult> UpdateAsync(Guid id, string? name, string? description);

        /// <summary>
        /// Deletes a tenant. Fails (not found = false) when users are still assigned to it.
        /// </summary>
        Task<EntityOperationResult> DeleteAsync(Guid id);

        /// <summary>
        /// Moves the user to the tenant (sets AppUser.TenantId). Idempotent.
        /// </summary>
        Task<EntityOperationResult> AssignUserAsync(Guid tenantId, Guid userId);
    }
}
