using AiSupport.Domain.Models;

namespace AiSupport.Application.Abstractions
{
    public interface IAppServiceRepository
    {
        Task<IEnumerable<AppService>> GetServiceByTenantIdAsync(Guid tenantId);

        Task<IEnumerable<AppService>> GetServicesByTenantIdsAsync(IEnumerable<Guid> tenantIds);
    }
}
