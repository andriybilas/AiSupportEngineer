using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;

namespace AiSupport.Application.Services
{
    public class AppUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;

        public AppUserService(
            IUserRepository userRepository,
            ITenantRepository tenantRepository,
            ISubscriptionRepository subscriptionRepository)
        {
            _userRepository = userRepository;
            _tenantRepository = tenantRepository;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<AppUserModel?> GetUserById(Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            var tenant = await _tenantRepository.GetByIdAsync(user.TenantId);

            AppTenantModel? tenantModel = null;
            var services = new List<AppServiceModel>();

            if (tenant != null)
            {
                var subscriptions = (await _subscriptionRepository.GetByTenantIdsAsync(new[] { tenant.Id }))
                    .ToList();

                tenantModel = MapTenant(tenant, subscriptions);
                services = MapActiveServices(subscriptions);
            }

            var userName = user.Name;

            if (string.IsNullOrWhiteSpace(userName))
            {
                userName = user.UserName ?? string.Empty;
            }

            return new AppUserModel
            {
                Id = user.Id,
                UserName = userName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Tenant = tenantModel,
                Services = services
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
            IEnumerable<Domain.Models.Subscription> allSubscriptions)
        {
            var subscriptionsForTenant = allSubscriptions
                .Where(s => s.TenantId == tenant.Id)
                .Select(MapSubscription)
                .ToList();

            return new AppTenantModel
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Description = tenant.Description,
                CreatedDateTime = tenant.CreatedDateTime,
                UpdatedDateTime = tenant.UpdatedDateTime,
                Subscriptions = subscriptionsForTenant
            };
        }

        private static List<AppServiceModel> MapActiveServices(IEnumerable<Domain.Models.Subscription> subscriptions)
        {
            return subscriptions
                .Where(s => s.Status == Domain.Models.SubscriptionStatus.Active)
                .SelectMany(s => s.AppServices)
                .DistinctBy(s => s.Id)
                .OrderBy(s => s.Name)
                .Select(MapService)
                .ToList();
        }

        private static SubscriptionModel MapSubscription(Domain.Models.Subscription subscription)
        {
            var services = subscription.AppServices
                .Select(MapService)
                .ToList();

            return new SubscriptionModel
            {
                Id = subscription.Id,
                TenantId = subscription.TenantId,
                Name = subscription.Name,
                Status = subscription.Status.ToString(),
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                CreatedDateTime = subscription.CreatedDateTime,
                UpdatedDateTime = subscription.UpdatedDateTime,
                AppServices = services
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



