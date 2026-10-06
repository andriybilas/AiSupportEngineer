using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;
using AiSupport.Domain.Models;
using AiSupport.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace AiSupport.Infrastructure.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly ApplicationDbContext _db;

        public TenantRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<AppTenant?> GetByIdAsync(Guid id)
        {
            return await _db.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<AppTenant>> GetAllAsync()
        {
            return await _db.Tenants
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<AppTenant> CreateAsync(string name, string description)
        {
            var now = DateTime.UtcNow;

            var entity = new AppTenant
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                CreatedDateTime = now,
                UpdatedDateTime = now
            };

            _db.Tenants.Add(entity);
            await _db.SaveChangesAsync();

            return entity;
        }

        public async Task<EntityOperationResult> UpdateAsync(Guid id, string? name, string? description)
        {
            var entity = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
            {
                return EntityOperationResult.NotFoundResult("Tenant not found.");
            }

            if (name != null)
            {
                entity.Name = name;
            }

            if (description != null)
            {
                entity.Description = description;
            }

            entity.UpdatedDateTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return EntityOperationResult.Ok();
        }

        public async Task<EntityOperationResult> DeleteAsync(Guid id)
        {
            var entity = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
            {
                return EntityOperationResult.NotFoundResult("Tenant not found.");
            }

            var hasUsers = await _db.Users.AnyAsync(u => u.TenantId == id);

            if (hasUsers)
            {
                return EntityOperationResult.Failed(
                    "Tenant still has users. Assign them to another tenant before deleting it.");
            }

            _db.Tenants.Remove(entity);
            await _db.SaveChangesAsync();

            return EntityOperationResult.Ok();
        }

        public async Task<EntityOperationResult> AssignUserAsync(Guid tenantId, Guid userId)
        {
            var tenantExists = await _db.Tenants.AnyAsync(t => t.Id == tenantId);

            if (!tenantExists)
            {
                return EntityOperationResult.NotFoundResult("Tenant not found.");
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return EntityOperationResult.NotFoundResult("User not found.");
            }

            if (user.TenantId == tenantId)
            {
                return EntityOperationResult.Ok();
            }

            user.TenantId = tenantId;
            user.UpdatedDateTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return EntityOperationResult.Ok();
        }
    }
}
