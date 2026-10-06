using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;
using AiSupport.Application.Services;
using Moq;

namespace AiSupport.Tests.Services
{
    public class TenantServiceTests
    {
        private readonly Mock<ITenantRepository> _tenantRepository = new(MockBehavior.Strict);
        private readonly TenantService _service;

        public TenantServiceTests()
        {
            _service = new TenantService(_tenantRepository.Object);
        }

        [Fact]
        public async Task AssignUserAsync_DelegatesToRepositoryAndReturnsItsResult()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _tenantRepository
                .Setup(r => r.AssignUserAsync(tenantId, userId))
                .ReturnsAsync(EntityOperationResult.NotFoundResult("User not found."));

            // Act
            var result = await _service.AssignUserAsync(tenantId, userId);

            // Assert
            Assert.True(result.NotFound);
            Assert.Contains("User not found.", result.Errors);

            _tenantRepository.Verify(
                r => r.AssignUserAsync(tenantId, userId),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenTenantStillHasUsers_ReturnsFailureFromRepository()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var message = "Tenant still has users. Assign them to another tenant before deleting it.";

            _tenantRepository
                .Setup(r => r.DeleteAsync(tenantId))
                .ReturnsAsync(EntityOperationResult.Failed(message));

            // Act
            var result = await _service.DeleteAsync(tenantId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.False(result.NotFound);
            Assert.Contains(message, result.Errors);
        }
    }
}
