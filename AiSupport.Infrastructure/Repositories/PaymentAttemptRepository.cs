using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;
using AiSupport.Domain.Models;
using AiSupport.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace AiSupport.Infrastructure.Repositories
{
    public class PaymentAttemptRepository : IPaymentAttemptRepository
    {
        private readonly ApplicationDbContext _db;

        public PaymentAttemptRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PaymentAttempt?> GetByIdAsync(Guid id)
        {
            return await _db.PaymentAttempts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PaymentAttempt>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _db.PaymentAttempts
                .Where(p => p.CustomerId == customerId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentAttempt>> GetByTenantIdAsync(Guid tenantId)
        {
            return await _db.PaymentAttempts
                .Where(p => p.Customer.TenantId == tenantId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<EntityCreateResult<PaymentAttempt>> CreateAsync(
            Guid customerId,
            string providerTransactionId,
            decimal amount,
            string currency,
            PaymentStatus status,
            string? errorCode)
        {
            var customerExists = await _db.Customers.AnyAsync(c => c.Id == customerId);

            if (!customerExists)
            {
                return EntityCreateResult<PaymentAttempt>.Failed("Customer not found.");
            }

            if (string.IsNullOrWhiteSpace(providerTransactionId))
            {
                return EntityCreateResult<PaymentAttempt>.Failed("ProviderTransactionId is required.");
            }

            if (string.IsNullOrWhiteSpace(currency))
            {
                return EntityCreateResult<PaymentAttempt>.Failed("Currency is required.");
            }

            var entity = new PaymentAttempt
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                ProviderTransactionId = providerTransactionId,
                Amount = amount,
                Currency = currency,
                Status = status,
                ErrorCode = errorCode,
                CreatedDateTime = DateTime.UtcNow
            };

            _db.PaymentAttempts.Add(entity);
            await _db.SaveChangesAsync();

            return EntityCreateResult<PaymentAttempt>.Ok(entity);
        }

        public async Task<EntityOperationResult> UpdateAsync(
            Guid id,
            string? providerTransactionId,
            decimal? amount,
            string? currency,
            PaymentStatus? status,
            string? errorCode)
        {
            var entity = await _db.PaymentAttempts.FirstOrDefaultAsync(p => p.Id == id);

            if (entity == null)
            {
                return EntityOperationResult.NotFoundResult("PaymentAttempt not found.");
            }

            if (providerTransactionId != null)
            {
                if (string.IsNullOrWhiteSpace(providerTransactionId))
                {
                    return EntityOperationResult.Failed("ProviderTransactionId cannot be empty.");
                }

                entity.ProviderTransactionId = providerTransactionId;
            }

            if (amount != null)
            {
                entity.Amount = amount.Value;
            }

            if (currency != null)
            {
                if (string.IsNullOrWhiteSpace(currency))
                {
                    return EntityOperationResult.Failed("Currency cannot be empty.");
                }

                entity.Currency = currency;
            }

            if (status != null)
            {
                entity.Status = status.Value;
            }

            if (errorCode != null)
            {
                entity.ErrorCode = errorCode;
            }

            await _db.SaveChangesAsync();

            return EntityOperationResult.Ok();
        }

        public async Task<EntityOperationResult> DeleteAsync(Guid id)
        {
            var entity = await _db.PaymentAttempts.FirstOrDefaultAsync(p => p.Id == id);

            if (entity == null)
            {
                return EntityOperationResult.NotFoundResult("PaymentAttempt not found.");
            }

            _db.PaymentAttempts.Remove(entity);
            await _db.SaveChangesAsync();

            return EntityOperationResult.Ok();
        }
    }
}
