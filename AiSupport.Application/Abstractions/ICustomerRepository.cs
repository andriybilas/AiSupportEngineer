using AiSupport.Application.Models;
using AiSupport.Domain.Models;

namespace AiSupport.Application.Abstractions
{
    public interface ICustomerRepository
    {
        /// <summary>
        /// Returns the customer only when it belongs to the given tenant;
        /// a customer of another tenant is reported the same way as a missing one (null).
        /// </summary>
        Task<Customer?> GetByIdAsync(Guid id, Guid tenantId);

        /// <summary>
        /// Returns all customers of the tenant, ordered by Name.
        /// </summary>
        Task<IReadOnlyList<Customer>> GetByTenantIdAsync(Guid tenantId);

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
