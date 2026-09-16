using AiSupport.Application.Models;
using AiSupport.Domain.Models;

namespace AiSupport.Application.Abstractions
{
    public interface IPaymentAttemptRepository
    {
        Task<PaymentAttempt?> GetByIdAsync(Guid id);

        Task<IEnumerable<PaymentAttempt>> GetByCustomerIdAsync(Guid customerId);

        Task<IEnumerable<PaymentAttempt>> GetByTenantIdAsync(Guid tenantId);

        Task<EntityCreateResult<PaymentAttempt>> CreateAsync(
            Guid customerId,
            string providerTransactionId,
            decimal amount,
            string currency,
            PaymentStatus status,
            string? errorCode);

        Task<EntityOperationResult> UpdateAsync(
            Guid id,
            string? providerTransactionId,
            decimal? amount,
            string? currency,
            PaymentStatus? status,
            string? errorCode);

        Task<EntityOperationResult> DeleteAsync(Guid id);
    }
}
