using AiSupport.Application.Models;
using AiSupport.Domain.Models;

namespace AiSupport.Application.Abstractions
{
    public interface ISubscriptionRepository
    {
        Task<IEnumerable<Subscription>> GetByTenantIdsAsync(IEnumerable<Guid> tenantIds);

        Task<Subscription?> GetByIdAsync(Guid id);

        Task<IEnumerable<Subscription>> GetAllAsync();

        Task<EntityCreateResult<Subscription>> CreateAsync(
            Guid tenantId,
            string name,
            SubscriptionStatus status,
            DateTime startDate,
            DateTime endDate,
            IEnumerable<Guid> appServiceIds);

        Task<EntityOperationResult> UpdateAsync(
            Guid id,
            string? name,
            SubscriptionStatus? status,
            DateTime? startDate,
            DateTime? endDate,
            IEnumerable<Guid>? appServiceIds);

        Task<EntityOperationResult> DeleteAsync(Guid id);
    }
}
