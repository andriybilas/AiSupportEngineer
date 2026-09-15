using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;
using AiSupport.Domain.Models;
using AiSupport.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace AiSupport.Infrastructure.Repositories
{
    public class AppServiceRepository : IAppServiceRepository
    {
        private readonly ApplicationDbContext _db;

        public AppServiceRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<AppService>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            var idList = ids?.ToList() ?? new List<Guid>();

            if (idList.Count == 0)
            {
                return new List<AppService>();
            }

            return await _db.AppServices
                .Where(s => idList.Contains(s.Id))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<AppService>> GetAllAsync()
        {
            return await _db.AppServices
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<AppService?> GetByIdAsync(Guid id)
        {
            return await _db.AppServices
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<AppService> CreateAsync(string name, decimal price, string description)
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

            return entity;
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
    }
}
