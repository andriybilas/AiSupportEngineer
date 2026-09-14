using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;

namespace AiSupport.Application.Services
{
    public class TenantService
    {
        private readonly ITenantRepository _tenantRepository;

        public TenantService(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public async Task<IEnumerable<AppTenantModel>> GetAllAsync()
        {
            var tenants = await _tenantRepository.GetAllAsync();

            return tenants
                .Select(Map)
                .ToList();
        }

        public async Task<AppTenantModel?> GetByIdAsync(Guid id)
        {
            var tenant = await _tenantRepository.GetByIdAsync(id);

            if (tenant == null)
            {
                return null;
            }

            return Map(tenant);
        }

        public async Task<EntityCreateResult<AppTenantModel>> CreateAsync(CreateTenantRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Name is required.");
            }

            if (errors.Count > 0)
            {
                return EntityCreateResult<AppTenantModel>.Failed(errors);
            }

            var description = request.Description ?? string.Empty;

            var created = await _tenantRepository.CreateAsync(
                request.Name.Trim(),
                description);

            return EntityCreateResult<AppTenantModel>.Ok(Map(created));
        }

        public async Task<EntityOperationResult> UpdateAsync(Guid id, UpdateTenantRequest request)
        {
            var hasAnyField =
                request.Name != null
                || request.Description != null;

            if (!hasAnyField)
            {
                return EntityOperationResult.Failed("At least one field must be provided.");
            }

            if (request.Name != null && string.IsNullOrWhiteSpace(request.Name))
            {
                return EntityOperationResult.Failed("Name cannot be empty.");
            }

            return await _tenantRepository.UpdateAsync(
                id,
                request.Name,
                request.Description);
        }

        public async Task<EntityOperationResult> DeleteAsync(Guid id)
        {
            return await _tenantRepository.DeleteAsync(id);
        }

        public async Task<EntityOperationResult> AddUserAsync(Guid tenantId, Guid userId)
        {
            return await _tenantRepository.AddUserAsync(tenantId, userId);
        }

        public async Task<EntityOperationResult> RemoveUserAsync(Guid tenantId, Guid userId)
        {
            return await _tenantRepository.RemoveUserAsync(tenantId, userId);
        }

        private static AppTenantModel Map(Domain.Models.AppTenant tenant)
        {
            return new AppTenantModel
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Description = tenant.Description,
                CreatedDateTime = tenant.CreatedDate,
                UpdatedDateTime = tenant.UpdatedDate,
                Subscriptions = Enumerable.Empty<SubscriptionModel>()
            };
        }
    }
}
