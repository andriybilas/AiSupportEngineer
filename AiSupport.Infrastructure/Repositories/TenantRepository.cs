using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;
using AiSupport.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using DomainAppTenant = AiSupport.Domain.Models.AppTenant;

namespace AiSupport.Infrastructure.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly ApplicationDbContext _db;

        public TenantRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<DomainAppTenant>> GetUserTenantsAsync(Guid userId)
        {
            var entities = await _db.Tenants
                .Where(t => t.AppUsers.Any(u => u.Id == userId))
                .AsNoTracking()
                .ToListAsync();

            return entities
                .Select(MapToDomain)
                .ToList();
        }

        public async Task<DomainAppTenant?> GetByIdAsync(Guid id)
        {
            var entity = await _db.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
            {
                return null;
            }

            return MapToDomain(entity);
        }

        public async Task<IEnumerable<DomainAppTenant>> GetAllAsync()
        {
            var entities = await _db.Tenants
                .AsNoTracking()
                .ToListAsync();

            return entities
                .Select(MapToDomain)
                .ToList();
        }

        public async Task<DomainAppTenant> CreateAsync(string name, string description)
        {
            var now = DateTime.UtcNow;

            var entity = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                CreatedDateTime = now,
                UpdatedDateTime = now
            };

            _db.Tenants.Add(entity);
            await _db.SaveChangesAsync();

            return MapToDomain(entity);
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

            _db.Tenants.Remove(entity);
            await _db.SaveChangesAsync();

            return EntityOperationResult.Ok();
        }

        public async Task<EntityOperationResult> AddUserAsync(Guid tenantId, Guid userId)
        {
            var tenant = await _db.Tenants
                .Include(t => t.AppUsers)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                return EntityOperationResult.NotFoundResult("Tenant not found.");
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return EntityOperationResult.NotFoundResult("User not found.");
            }

            var alreadyMember = tenant.AppUsers.Any(u => u.Id == userId);

            if (alreadyMember)
            {
                return EntityOperationResult.Failed("User is already a member of this tenant.");
            }

            tenant.AppUsers.Add(user);
            tenant.UpdatedDateTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return EntityOperationResult.Ok();
        }

        public async Task<EntityOperationResult> RemoveUserAsync(Guid tenantId, Guid userId)
        {
            var tenant = await _db.Tenants
                .Include(t => t.AppUsers)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                return EntityOperationResult.NotFoundResult("Tenant not found.");
            }

            var user = tenant.AppUsers.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return EntityOperationResult.NotFoundResult("User is not a member of this tenant.");
            }

            tenant.AppUsers.Remove(user);
            tenant.UpdatedDateTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return EntityOperationResult.Ok();
        }

        private static DomainAppTenant MapToDomain(Tenant entity)
        {
            return new DomainAppTenant
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                CreatedDate = entity.CreatedDateTime,
                UpdatedDate = entity.UpdatedDateTime,
                Subscriptions = new List<AiSupport.Domain.Models.Subscription>()
            };
        }
    }
}
