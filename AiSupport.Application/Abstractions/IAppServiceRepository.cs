using AiSupport.Application.Models;
using AiSupport.Domain.Models;

namespace AiSupport.Application.Abstractions
{
    public interface IAppServiceRepository
    {
        Task<IEnumerable<AppService>> GetByIdsAsync(IEnumerable<Guid> ids);

        Task<IEnumerable<AppService>> GetAllAsync();

        Task<AppService?> GetByIdAsync(Guid id);

        Task<AppService> CreateAsync(string name, decimal price, string description);

        Task<EntityOperationResult> UpdateAsync(Guid id, string? name, decimal? price, string? description);

        Task<EntityOperationResult> DeleteAsync(Guid id);
    }
}
