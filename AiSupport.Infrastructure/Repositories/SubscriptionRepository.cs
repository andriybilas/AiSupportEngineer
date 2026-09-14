using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;
using AiSupport.Domain.Models;
using AiSupport.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using DomainSubscription = AiSupport.Domain.Models.Subscription;
using DomainAppService = AiSupport.Domain.Models.AppService;
using EfSubscription = AiSupport.Infrastructure.DataBase.Subscription;
using EfAppService = AiSupport.Infrastructure.DataBase.AppService;

namespace AiSupport.Infrastructure.Repositories
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ApplicationDbContext _db;

        public SubscriptionRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<DomainSubscription>> GetByTenantIdsAsync(IEnumerable<Guid> tenantIds)
        {
            var ids = tenantIds?.ToList() ?? new List<Guid>();

            if (ids.Count == 0)
            {
                return new List<DomainSubscription>();
            }

            var entities = await _db.Subscriptions
                .Where(s => ids.Contains(s.TenantId))
                .Include(s => s.AppServices)
                .AsNoTracking()
                .ToListAsync();

            return entities
                .Select(MapToDomain)
                .ToList();
        }

        public async Task<DomainSubscription?> GetByIdAsync(Guid id)
        {
            var entity = await _db.Subscriptions
                .Include(s => s.AppServices)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (entity == null)
            {
                return null;
            }

            return MapToDomain(entity);
        }

        public async Task<IEnumerable<DomainSubscription>> GetAllAsync()
        {
            var entities = await _db.Subscriptions
                .Include(s => s.AppServices)
                .AsNoTracking()
                .ToListAsync();

            return entities
                .Select(MapToDomain)
                .ToList();
        }

        public async Task<EntityCreateResult<DomainSubscription>> CreateAsync(
            Guid tenantId,
            string name,
            SubscriptionStatus status,
            DateTime startDate,
            DateTime endDate,
            IEnumerable<Guid> appServiceIds)
        {
            var tenantExists = await _db.Tenants.AnyAsync(t => t.Id == tenantId);

            if (!tenantExists)
            {
                return EntityCreateResult<DomainSubscription>.Failed("Tenant not found.");
            }

            var serviceIdList = appServiceIds?.Distinct().ToList() ?? new List<Guid>();
            var services = new List<EfAppService>();

            if (serviceIdList.Count > 0)
            {
                services = await _db.AppServices
                    .Where(s => serviceIdList.Contains(s.Id))
                    .ToListAsync();

                if (services.Count != serviceIdList.Count)
                {
                    return EntityCreateResult<DomainSubscription>.Failed(
                        "One or more AppServiceIds do not exist.");
                }
            }

            var now = DateTime.UtcNow;

            var entity = new EfSubscription
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = name,
                Status = status.ToString(),
                StartDate = startDate,
                EndDate = endDate,
                CreatedDateTime = now,
                UpdatedDateTime = now,
                AppServices = services
            };

            _db.Subscriptions.Add(entity);
            await _db.SaveChangesAsync();

            return EntityCreateResult<DomainSubscription>.Ok(MapToDomain(entity));
        }

        public async Task<EntityOperationResult> UpdateAsync(
            Guid id,
            string? name,
            SubscriptionStatus? status,
            DateTime? startDate,
            DateTime? endDate,
            IEnumerable<Guid>? appServiceIds)
        {
            var entity = await _db.Subscriptions
                .Include(s => s.AppServices)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (entity == null)
            {
                return EntityOperationResult.NotFoundResult("Subscription not found.");
            }

            if (name != null)
            {
                entity.Name = name;
            }

            if (status != null)
            {
                entity.Status = status.Value.ToString();
            }

            if (startDate != null)
            {
                entity.StartDate = startDate.Value;
            }

            if (endDate != null)
            {
                entity.EndDate = endDate.Value;
            }

            if (appServiceIds != null)
            {
                var serviceIdList = appServiceIds.Distinct().ToList();
                var services = new List<EfAppService>();

                if (serviceIdList.Count > 0)
                {
                    services = await _db.AppServices
                        .Where(s => serviceIdList.Contains(s.Id))
                        .ToListAsync();

                    if (services.Count != serviceIdList.Count)
                    {
                        return EntityOperationResult.Failed(
                            "One or more AppServiceIds do not exist.");
                    }
                }

                entity.AppServices.Clear();

                foreach (var service in services)
                {
                    entity.AppServices.Add(service);
                }
            }

            entity.UpdatedDateTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return EntityOperationResult.Ok();
        }

        public async Task<EntityOperationResult> DeleteAsync(Guid id)
        {
            var entity = await _db.Subscriptions.FirstOrDefaultAsync(s => s.Id == id);

            if (entity == null)
            {
                return EntityOperationResult.NotFoundResult("Subscription not found.");
            }

            _db.Subscriptions.Remove(entity);
            await _db.SaveChangesAsync();

            return EntityOperationResult.Ok();
        }

        private static DomainSubscription MapToDomain(EfSubscription entity)
        {
            var status = ParseStatus(entity.Status);

            var services = entity.AppServices
                .Select(MapService)
                .ToList();

            return new DomainSubscription
            {
                Id = entity.Id,
                TenantId = entity.TenantId,
                Name = entity.Name,
                Status = status,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                CreatedDate = entity.CreatedDateTime,
                UpdatedDate = entity.UpdatedDateTime,
                AppServices = services
            };
        }

        private static DomainAppService MapService(EfAppService entity)
        {
            return new DomainAppService
            {
                Id = entity.Id,
                Name = entity.Name,
                Price = entity.Price,
                Description = entity.Description ?? string.Empty
            };
        }

        private static SubscriptionStatus ParseStatus(string status)
        {
            if (Enum.TryParse<SubscriptionStatus>(status, ignoreCase: true, out var parsed))
            {
                return parsed;
            }

            return SubscriptionStatus.Expired;
        }
    }
}
