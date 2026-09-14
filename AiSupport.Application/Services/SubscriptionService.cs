using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;
using AiSupport.Domain.Models;

namespace AiSupport.Application.Services
{
    public class SubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<IEnumerable<SubscriptionModel>> GetAllAsync(Guid? tenantId = null)
        {
            IEnumerable<Subscription> subscriptions;

            if (tenantId.HasValue)
            {
                subscriptions = await _subscriptionRepository.GetByTenantIdsAsync(
                    new[] { tenantId.Value });
            }
            else
            {
                subscriptions = await _subscriptionRepository.GetAllAsync();
            }

            return subscriptions
                .Select(Map)
                .ToList();
        }

        public async Task<SubscriptionModel?> GetByIdAsync(Guid id)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(id);

            if (subscription == null)
            {
                return null;
            }

            return Map(subscription);
        }

        public async Task<EntityCreateResult<SubscriptionModel>> CreateAsync(CreateSubscriptionRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                errors.Add("Name is required.");
            }

            if (request.TenantId == Guid.Empty)
            {
                errors.Add("TenantId is required.");
            }

            if (request.EndDate < request.StartDate)
            {
                errors.Add("EndDate cannot be earlier than StartDate.");
            }

            if (errors.Count > 0)
            {
                return EntityCreateResult<SubscriptionModel>.Failed(errors);
            }

            var appServiceIds = request.AppServiceIds ?? new List<Guid>();

            var createResult = await _subscriptionRepository.CreateAsync(
                request.TenantId,
                request.Name.Trim(),
                request.Status,
                request.StartDate,
                request.EndDate,
                appServiceIds);

            if (!createResult.Succeeded)
            {
                return EntityCreateResult<SubscriptionModel>.Failed(createResult.Errors);
            }

            return EntityCreateResult<SubscriptionModel>.Ok(Map(createResult.Entity!));
        }

        public async Task<EntityOperationResult> UpdateAsync(Guid id, UpdateSubscriptionRequest request)
        {
            var hasAnyField =
                request.Name != null
                || request.Status != null
                || request.StartDate != null
                || request.EndDate != null
                || request.AppServiceIds != null;

            if (!hasAnyField)
            {
                return EntityOperationResult.Failed("At least one field must be provided.");
            }

            if (request.Name != null && string.IsNullOrWhiteSpace(request.Name))
            {
                return EntityOperationResult.Failed("Name cannot be empty.");
            }

            if (request.StartDate != null && request.EndDate != null && request.EndDate < request.StartDate)
            {
                return EntityOperationResult.Failed("EndDate cannot be earlier than StartDate.");
            }

            return await _subscriptionRepository.UpdateAsync(
                id,
                request.Name,
                request.Status,
                request.StartDate,
                request.EndDate,
                request.AppServiceIds);
        }

        public async Task<EntityOperationResult> DeleteAsync(Guid id)
        {
            return await _subscriptionRepository.DeleteAsync(id);
        }

        private static SubscriptionModel Map(Subscription subscription)
        {
            var services = subscription.AppServices
                .Select(s => new AppServiceModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    Description = s.Description
                })
                .ToList();

            return new SubscriptionModel
            {
                Id = subscription.Id,
                TenantId = subscription.TenantId,
                Name = subscription.Name,
                Status = subscription.Status.ToString(),
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                CreatedDateTime = subscription.CreatedDate,
                UpdatedDateTime = subscription.UpdatedDate,
                AppServices = services
            };
        }
    }
}
