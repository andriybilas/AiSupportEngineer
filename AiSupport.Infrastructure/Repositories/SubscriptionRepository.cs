using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;
using AiSupport.Domain.Models;
using AiSupport.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace AiSupport.Infrastructure.Repositories
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ApplicationDbContext _db;

        public SubscriptionRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Subscription>> GetByTenantIdsAsync(IEnumerable<Guid> tenantIds)
        {
            var ids = tenantIds?.ToList() ?? new List<Guid>();

            if (ids.Count == 0)
            {
                return new List<Subscription>();
            }

            return await _db.Subscriptions
                .Where(s => ids.Contains(s.TenantId))
                .Include(s => s.AppServices)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Subscription?> GetByIdAsync(Guid id)
        {
            return await _db.Subscriptions
                .Include(s => s.AppServices)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Subscription>> GetAllAsync()
        {
            return await _db.Subscriptions
                .Include(s => s.AppServices)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<EntityCreateResult<Subscription>> CreateAsync(
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
                return EntityCreateResult<Subscription>.Failed("Tenant not found.");
            }

            var serviceIdList = appServiceIds?.Distinct().ToList() ?? new List<Guid>();
            var services = new List<AppService>();

            if (serviceIdList.Count > 0)
            {
                services = await _db.AppServices
                    .Where(s => serviceIdList.Contains(s.Id))
                    .ToListAsync();

                if (services.Count != serviceIdList.Count)
                {
                    return EntityCreateResult<Subscription>.Failed(
                        "One or more AppServiceIds do not exist.");
                }
            }

            var now = DateTime.UtcNow;

            var entity = new Subscription
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = name,
                Status = status,
                StartDate = startDate,
                EndDate = endDate,
                CreatedDateTime = now,
                UpdatedDateTime = now,
                AppServices = services
            };

            _db.Subscriptions.Add(entity);
            await _db.SaveChangesAsync();

            return EntityCreateResult<Subscription>.Ok(entity);
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
                entity.Status = status.Value;
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
                var services = new List<AppService>();

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
    }
}
