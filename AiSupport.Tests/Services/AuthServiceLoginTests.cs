using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;
using AiSupport.Application.Services;
using Moq;

namespace AiSupport.Tests.Services
{
    public class AuthServiceLoginTests
    {
        private readonly Mock<IUserRepository> _userRepository = new(MockBehavior.Strict);
        private readonly Mock<ITenantRepository> _tenantRepository = new(MockBehavior.Strict);
        private readonly Mock<IJwtTokenService> _jwtTokenService = new(MockBehavior.Strict);
        private readonly AuthService _service;

        public AuthServiceLoginTests()
        {
            _service = new AuthService(
                _userRepository.Object,
                _tenantRepository.Object,
                _jwtTokenService.Object);
        }

        [Fact]
        public async Task LoginAsync_WhenCredentialsAreValid_CreatesTokenForUsersTenant()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var expiresAt = new DateTime(2026, 10, 7, 10, 0, 0, DateTimeKind.Utc);

            _userRepository
                .Setup(r => r.ValidateCredentialsAsync("admin@aisupport.local", "Passw0rd!"))
                .ReturnsAsync(CredentialValidationResult.Ok(userId, "admin@aisupport.local", tenantId));

            _jwtTokenService
                .Setup(s => s.CreateToken(userId, "admin@aisupport.local", tenantId))
                .Returns(new JwtTokenResult { Token = "jwt-token", ExpiresAt = expiresAt });

            var request = new LoginRequest
            {
                UserNameOrEmail = " admin@aisupport.local ",
                Password = "Passw0rd!"
            };

            // Act
            var result = await _service.LoginAsync(request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("jwt-token", result.Token);
            Assert.Equal(userId, result.UserId);

            _jwtTokenService.Verify(
                s => s.CreateToken(userId, "admin@aisupport.local", tenantId),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WhenCredentialsAreInvalid_ReturnsFailureWithoutToken()
        {
            // Arrange
            _userRepository
                .Setup(r => r.ValidateCredentialsAsync("admin@aisupport.local", "wrong"))
                .ReturnsAsync(CredentialValidationResult.Failed());

            var request = new LoginRequest
            {
                UserNameOrEmail = "admin@aisupport.local",
                Password = "wrong"
            };

            // Act
            var result = await _service.LoginAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Token);

            _jwtTokenService.VerifyNoOtherCalls();
        }
    }
}
