using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;
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

        public async Task<IEnumerable<DomainAppService>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            var idList = ids?.ToList() ?? new List<Guid>();

            if (idList.Count == 0)
            {
                return new List<DomainAppService>();
            }

            var entities = await _db.AppServices
                .Where(s => idList.Contains(s.Id))
                .AsNoTracking()
                .ToListAsync();

            return entities
                .Select(MapToDomain)
                .ToList();
        }

        public async Task<IEnumerable<DomainAppService>> GetAllAsync()
        {
            var entities = await _db.AppServices
                .AsNoTracking()
                .ToListAsync();

            return entities
                .Select(MapToDomain)
                .ToList();
        }

        public async Task<DomainAppService?> GetByIdAsync(Guid id)
        {
            var entity = await _db.AppServices
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (entity == null)
            {
                return null;
            }

            return MapToDomain(entity);
        }

        public async Task<DomainAppService> CreateAsync(string name, decimal price, string description)
        {
            var entity = new AppService
            {
                Id = Guid.NewGuid(),
                Name = name,
                Price = price,
                Description = description
            };

            _db.AppServices.Add(entity);
            await _db.SaveChangesAsync();

            return MapToDomain(entity);
        }

        public async Task<EntityOperationResult> UpdateAsync(
            Guid id,
            string? name,
            decimal? price,
            string? description)
        {
            var entity = await _db.AppServices.FirstOrDefaultAsync(s => s.Id == id);

            if (entity == null)
            {
                return EntityOperationResult.NotFoundResult("AppService not found.");
            }

            if (name != null)
            {
                entity.Name = name;
            }

            if (price != null)
            {
                entity.Price = price.Value;
            }

            if (description != null)
            {
                entity.Description = description;
            }

            await _db.SaveChangesAsync();

            return EntityOperationResult.Ok();
        }

        public async Task<EntityOperationResult> DeleteAsync(Guid id)
        {
            var entity = await _db.AppServices.FirstOrDefaultAsync(s => s.Id == id);

            if (entity == null)
            {
                return EntityOperationResult.NotFoundResult("AppService not found.");
            }

            _db.AppServices.Remove(entity);
            await _db.SaveChangesAsync();

            return EntityOperationResult.Ok();
        }

        private static DomainAppService MapToDomain(AppService entity)
        {
            return new DomainAppService
            {
                Id = entity.Id,
                Name = entity.Name,
                Price = entity.Price,
                Description = entity.Description ?? string.Empty
            };
        }
    }
}
