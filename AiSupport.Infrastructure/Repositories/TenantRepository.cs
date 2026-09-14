using AiSupport.Application.Abstractions;
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

        private static DomainAppTenant MapToDomain(Tenant entity)
        {
            return new DomainAppTenant
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                CreatedDate = entity.CreatedDateTime,
                UpdatedDate = entity.UpdatedDateTime,
                AppServices = new List<AiSupport.Domain.Models.AppService>()
            };
        }
    }
}
