using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;

namespace AiSupport.Application.Services
{
    public class AppUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IAppServiceRepository _appServiceRepository;

        public AppUserService(
            IUserRepository userRepository,
            ITenantRepository tenantRepository,
            IAppServiceRepository appServiceRepository)
        {
            _userRepository = userRepository;
            _tenantRepository = tenantRepository;
            _appServiceRepository = appServiceRepository;
        }

        public async Task<RegisterUserResult> RegisterAsync(RegisterUserRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                errors.Add("UserName is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                errors.Add("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                errors.Add("Password is required.");
            }

            if (errors.Count > 0)
            {
                return RegisterUserResult.Failed(errors);
            }

            var firstName = request.FirstName ?? string.Empty;
            var lastName = request.LastName ?? string.Empty;

            var creationResult = await _userRepository.CreateUserAsync(
                request.UserName,
                firstName,
                lastName,
                request.Email,
                request.Password);

            if (!creationResult.Succeeded)
            {
                return RegisterUserResult.Failed(creationResult.Errors);
            }

            return RegisterUserResult.Ok(creationResult.UserId!.Value);
        }

        public async Task<AppUserModel?> GetUserById(Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            var userTenants = await _tenantRepository.GetUserTenantsAsync(user.Id);

            var tenantIds = userTenants
                .Select(t => t.Id)
                .ToList();

            var appServices = await _appServiceRepository.GetServicesByTenantIdsAsync(tenantIds);

            var tenants = userTenants
                .Select(ut => MapTenant(ut, appServices))
                .ToList();

            return new AppUserModel
            {
                Id = user.Id,
                UserName = user.Name,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Tenants = tenants
            };
        }

        public async Task<UpdateUserResult> UpdateUserAsync(Guid userId, UpdateUserRequest request)
        {
            var existingUser = await _userRepository.GetUserByIdAsync(userId);

            if (existingUser == null)
            {
                return UpdateUserResult.NotFound();
            }

            var hasAnyField =
                request.UserName != null
                || request.Email != null
                || request.FirstName != null
                || request.LastName != null;

            if (!hasAnyField)
            {
                return UpdateUserResult.Failed("At least one field must be provided.");
            }

            var updateResult = await _userRepository.UpdateUserAsync(
                userId,
                request.UserName,
                request.Email,
                request.FirstName,
                request.LastName);

            if (updateResult.UserNotFound)
            {
                return UpdateUserResult.NotFound();
            }

            if (!updateResult.Succeeded)
            {
                return UpdateUserResult.Failed(updateResult.Errors);
            }

            return UpdateUserResult.Ok();
        }

        private static AppTenantModel MapTenant(
            Domain.Models.AppTenant tenant,
            IEnumerable<Domain.Models.AppService> allServices)
        {
            var servicesForTenant = allServices
                .Where(s => s.TenantId == tenant.Id)
                .Select(MapService)
                .ToList();

            return new AppTenantModel
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Description = tenant.Description,
                CreatedDateTime = tenant.CreatedDate,
                UpdatedDateTime = tenant.UpdatedDate,
                AppServices = servicesForTenant
            };
        }

        private static AppServiceModel MapService(Domain.Models.AppService service)
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
