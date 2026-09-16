using AiSupport.Application.Models;
using AiSupport.Domain.Models;

namespace AiSupport.Application.Abstractions
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(Guid id);

        Task<IEnumerable<Customer>> GetByTenantIdAsync(Guid tenantId);

        Task<IEnumerable<Customer>> GetAllAsync();

        Task<EntityCreateResult<Customer>> CreateAsync(
            Guid tenantId,
            string name,
            string? externalId);

        Task<EntityOperationResult> UpdateAsync(
            Guid id,
            string? name,
            string? externalId);

        Task<EntityOperationResult> DeleteAsync(Guid id);
    }
}
