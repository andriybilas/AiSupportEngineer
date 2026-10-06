using AiSupport.Application.Abstractions;
using AiSupport.Application.Services;
using AiSupport.Domain.Models;
using Moq;

namespace AiSupport.Tests.Services
{
    public class AppUserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepository = new(MockBehavior.Strict);
        private readonly Mock<ITenantRepository> _tenantRepository = new(MockBehavior.Strict);
        private readonly Mock<ISubscriptionRepository> _subscriptionRepository = new(MockBehavior.Strict);
        private readonly AppUserService _service;

        public AppUserServiceTests()
        {
            _service = new AppUserService(
                _userRepository.Object,
                _tenantRepository.Object,
                _subscriptionRepository.Object);
        }

        [Fact]
        public async Task GetUserById_WhenUserExists_ReturnsSingleTenantAndServicesOfActiveSubscriptions()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var user = CreateUser(tenantId);
            var tenant = new AppTenant { Id = tenantId, Name = "Acme Retail", Description = "Retailer" };

            var monitoring = CreateService("Payments Monitoring");
            var bot = CreateService("Customer Support Bot");
            var fraud = CreateService("Fraud Detection");

            var active = CreateSubscription(tenantId, "Growth", SubscriptionStatus.Active, monitoring, bot);
            var activeAddOn = CreateSubscription(tenantId, "Add-on", SubscriptionStatus.Active, bot);
            var expired = CreateSubscription(tenantId, "Pilot", SubscriptionStatus.Expired, fraud);

            _userRepository
                .Setup(r => r.GetUserByIdAsync(user.Id))
                .ReturnsAsync(user);

            _tenantRepository
                .Setup(r => r.GetByIdAsync(tenantId))
                .ReturnsAsync(tenant);

            _subscriptionRepository
                .Setup(r => r.GetByTenantIdsAsync(It.Is<IEnumerable<Guid>>(ids => ids.Single() == tenantId)))
                .ReturnsAsync(new List<Subscription> { active, activeAddOn, expired });

            // Act
            var result = await _service.GetUserById(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Tenant);
            Assert.Equal(tenantId, result.Tenant.Id);
            Assert.Equal("Acme Retail", result.Tenant.Name);
            Assert.Equal(3, result.Tenant.Subscriptions.Count());

            var serviceNames = result.Services
                .Select(s => s.Name)
                .ToList();

            Assert.Equal(new[] { "Customer Support Bot", "Payments Monitoring" }, serviceNames);
        }

        [Fact]
        public async Task GetUserById_WhenUserMissing_ReturnsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userRepository
                .Setup(r => r.GetUserByIdAsync(userId))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _service.GetUserById(userId);

            // Assert
            Assert.Null(result);

            _tenantRepository.VerifyNoOtherCalls();
            _subscriptionRepository.VerifyNoOtherCalls();
        }

        private static AppUser CreateUser(Guid tenantId)
        {
            return new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = "demo@aisupport.local",
                Email = "demo@aisupport.local",
                Name = "demo@aisupport.local",
                TenantId = tenantId
            };
        }

        private static AppService CreateService(string name)
        {
            return new AppService
            {
                Id = Guid.NewGuid(),
                Name = name,
                Price = 100m
            };
        }

        private static Subscription CreateSubscription(
            Guid tenantId,
            string name,
            SubscriptionStatus status,
            params AppService[] services)
        {
            return new Subscription
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = name,
                Status = status,
                StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                AppServices = services.ToList()
            };
        }
    }
}
