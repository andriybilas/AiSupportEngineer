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

        public async Task<IEnumerable<AppTenant>> GetUserTenantsAsync(Guid userId)
        {
            return await _db.Tenants
                .Where(t => t.AppUsers.Any(u => u.Id == userId))
                .AsNoTracking()
                .ToListAsync();
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
    }
}
