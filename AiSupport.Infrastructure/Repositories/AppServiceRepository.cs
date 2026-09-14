using AiSupport.Application.Abstractions;
using AiSupport.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using DomainAppService = AiSupport.Domain.Models.AppService;

namespace AiSupport.Infrastructure.Repositories
{
    public class AppServiceRepository : IAppServiceRepository
    {
        private readonly ApplicationDbContext _db;

        public AppServiceRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<DomainAppService>> GetServiceByTenantIdAsync(Guid tenantId)
        {
            var entities = await _db.AppServices
                .Where(s => s.TenantId == tenantId)
                .AsNoTracking()
                .ToListAsync();

            return entities
                .Select(MapToDomain)
                .ToList();
        }

        public async Task<IEnumerable<DomainAppService>> GetServicesByTenantIdsAsync(IEnumerable<Guid> tenantIds)
        {
            var ids = tenantIds?.ToList() ?? new List<Guid>();

            if (ids.Count == 0)
            {
                return new List<DomainAppService>();
            }

            var entities = await _db.AppServices
                .Where(s => s.TenantId.HasValue && ids.Contains(s.TenantId.Value))
                .AsNoTracking()
                .ToListAsync();

            return entities
                .Select(MapToDomain)
                .ToList();
        }

        private static DomainAppService MapToDomain(AppService entity)
        {
            return new DomainAppService
            {
                Id = entity.Id,
                Name = entity.Name,
                Price = entity.Price,
                Description = entity.Description ?? string.Empty,
                TenantId = entity.TenantId ?? Guid.Empty,
                StartDate = DateTime.MinValue,
                EndDate = DateTime.MinValue
            };
        }
    }
}
