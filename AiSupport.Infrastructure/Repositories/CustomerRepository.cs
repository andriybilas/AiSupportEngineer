using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;
using AiSupport.Domain.Models;
using AiSupport.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace AiSupport.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _db;

        public CustomerRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Customer?> GetByIdAsync(Guid id)
        {
            return await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Customer>> GetByTenantIdAsync(Guid tenantId)
        {
            return await _db.Customers
                .Where(c => c.TenantId == tenantId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _db.Customers
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<EntityCreateResult<Customer>> CreateAsync(
            Guid tenantId,
            string name,
            string? externalId)
        {
            var tenantExists = await _db.Tenants.AnyAsync(t => t.Id == tenantId);

            if (!tenantExists)
            {
                return EntityCreateResult<Customer>.Failed("Tenant not found.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return EntityCreateResult<Customer>.Failed("Name is required.");
            }

            var now = DateTime.UtcNow;

            var entity = new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = name,
                ExternalId = externalId,
                CreatedDateTime = now,
                UpdatedDateTime = now
            };

            _db.Customers.Add(entity);
            await _db.SaveChangesAsync();

            return EntityCreateResult<Customer>.Ok(entity);
        }

        public async Task<EntityOperationResult> UpdateAsync(
            Guid id,
            string? name,
            string? externalId)
        {
            var entity = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);

            if (entity == null)
            {
                return EntityOperationResult.NotFoundResult("Customer not found.");
            }

            if (name != null)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return EntityOperationResult.Failed("Name cannot be empty.");
                }

                entity.Name = name;
            }

            if (externalId != null)
            {
                entity.ExternalId = externalId;
            }

            entity.UpdatedDateTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return EntityOperationResult.Ok();
        }

        public async Task<EntityOperationResult> DeleteAsync(Guid id)
        {
            var entity = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);

            if (entity == null)
            {
                return EntityOperationResult.NotFoundResult("Customer not found.");
            }

            _db.Customers.Remove(entity);
            await _db.SaveChangesAsync();

            return EntityOperationResult.Ok();
        }
    }
}
