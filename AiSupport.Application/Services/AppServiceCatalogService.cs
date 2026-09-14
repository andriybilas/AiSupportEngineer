using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;

namespace AiSupport.Application.Services
{
    public class AppServiceCatalogService
    {
        private readonly IAppServiceRepository _appServiceRepository;

        public AppServiceCatalogService(IAppServiceRepository appServiceRepository)
        {
            _appServiceRepository = appServiceRepository;
        }

        public async Task<IEnumerable<AppServiceModel>> GetAllAsync()
        {
            var services = await _appServiceRepository.GetAllAsync();

            return services
                .Select(Map)
                .ToList();
        }

        public async Task<AppServiceModel?> GetByIdAsync(Guid id)
        {
            var service = await _appServiceRepository.GetByIdAsync(id);

            if (service == null)
            {
                return null;
            }

            return Map(service);
        }

        public async Task<EntityCreateResult<AppServiceModel>> CreateAsync(CreateAppServiceRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Name is required.");
            }

            if (request.Price < 0)
            {
                errors.Add("Price cannot be negative.");
            }

            if (errors.Count > 0)
            {
                return EntityCreateResult<AppServiceModel>.Failed(errors);
            }

            var description = request.Description ?? string.Empty;

            var created = await _appServiceRepository.CreateAsync(
                request.Name.Trim(),
                request.Price,
                description);

            return EntityCreateResult<AppServiceModel>.Ok(Map(created));
        }

        public async Task<EntityOperationResult> UpdateAsync(Guid id, UpdateAppServiceRequest request)
        {
            var hasAnyField =
                request.Name != null
                || request.Price != null
                || request.Description != null;

            if (!hasAnyField)
            {
                return EntityOperationResult.Failed("At least one field must be provided.");
            }

            if (request.Price != null && request.Price < 0)
            {
                return EntityOperationResult.Failed("Price cannot be negative.");
            }

            return await _appServiceRepository.UpdateAsync(
                id,
                request.Name,
                request.Price,
                request.Description);
        }

        public async Task<EntityOperationResult> DeleteAsync(Guid id)
        {
            return await _appServiceRepository.DeleteAsync(id);
        }

        private static AppServiceModel Map(Domain.Models.AppService service)
        {
            return new AppServiceModel
            {
                Id = service.Id,
                Name = service.Name,
                Price = service.Price,
                Description = service.Description
            };
        }
    }
}
