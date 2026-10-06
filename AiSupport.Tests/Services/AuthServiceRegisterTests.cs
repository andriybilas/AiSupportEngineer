using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;
using AiSupport.Application.Services;
using AiSupport.Domain.Models;
using Moq;

namespace AiSupport.Tests.Services
{
    public class AuthServiceRegisterTests
    {
        private const string Email = "jane.doe@example.com";
        private const string Password = "Str0ng!Pass";
        private const string TenantName = "Acme";

        private readonly Mock<IUserRepository> _userRepository = new(MockBehavior.Strict);
        private readonly Mock<ITenantRepository> _tenantRepository = new(MockBehavior.Strict);
        private readonly Mock<IJwtTokenService> _jwtTokenService = new(MockBehavior.Strict);
        private readonly AuthService _service;

        public AuthServiceRegisterTests()
        {
            _service = new AuthService(
                _userRepository.Object,
                _tenantRepository.Object,
                _jwtTokenService.Object);
        }

        [Fact]
        public async Task RegisterAsync_WhenRequestIsValid_CreatesUserInTenantAndReturnsToken()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var expiresAt = new DateTime(2026, 10, 7, 10, 0, 0, DateTimeKind.Utc);
            var request = CreateRequest(tenantId);
            request.Email = "  " + Email + "  ";

            SetupTenantExists(tenantId);
            SetupEmailRegistered(false);

            _userRepository
                .Setup(r => r.CreateUserAsync(
                    Email,
                    string.Empty,
                    string.Empty,
                    Email,
                    Password,
                    tenantId))
                .ReturnsAsync(UserCreationResult.Ok(userId));

            _jwtTokenService
                .Setup(s => s.CreateToken(userId, Email, tenantId))
                .Returns(new JwtTokenResult { Token = "jwt-token", ExpiresAt = expiresAt });

            // Act
            var result = await _service.RegisterAsync(request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.False(result.NotFound);
            Assert.False(result.Conflict);
            Assert.Empty(result.Errors);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(Email, result.Email);
            Assert.Equal(tenantId, result.TenantId);
            Assert.Equal(TenantName, result.TenantName);
            Assert.Equal("jwt-token", result.Token);
            Assert.Equal(expiresAt, result.ExpiresAt);

            _userRepository.Verify(
                r => r.CreateUserAsync(
                    Email,
                    string.Empty,
                    string.Empty,
                    Email,
                    Password,
                    tenantId),
                Times.Once);

            _jwtTokenService.Verify(
                s => s.CreateToken(userId, Email, tenantId),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WhenPasswordsDoNotMatch_ReturnsValidationFailure()
        {
            // Arrange
            var request = CreateRequest(Guid.NewGuid());
            request.ConfirmPassword = "Different!Pass1";

            // Act
            var result = await _service.RegisterAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.False(result.NotFound);
            Assert.False(result.Conflict);
            Assert.Contains("Passwords do not match.", result.Errors);

            VerifyNoDependencyCalls();
        }

        [Theory]
        [InlineData("", "Email is required.")]
        [InlineData("not-an-email", "Email format is invalid.")]
        [InlineData("Jane <jane.doe@example.com>", "Email format is invalid.")]
        public async Task RegisterAsync_WhenEmailIsMissingOrInvalid_ReturnsValidationFailure(
            string email,
            string expectedError)
        {
            // Arrange
            var request = CreateRequest(Guid.NewGuid());
            request.Email = email;

            // Act
            var result = await _service.RegisterAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains(expectedError, result.Errors);

            VerifyNoDependencyCalls();
        }

        [Fact]
        public async Task RegisterAsync_WhenTenantIdIsEmpty_ReturnsValidationFailure()
        {
            // Arrange
            var request = CreateRequest(Guid.Empty);

            // Act
            var result = await _service.RegisterAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.False(result.NotFound);
            Assert.Contains("TenantId is required.", result.Errors);

            VerifyNoDependencyCalls();
        }

        [Fact]
        public async Task RegisterAsync_WhenTenantMissing_ReturnsNotFound()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var request = CreateRequest(tenantId);

            _tenantRepository
                .Setup(r => r.GetByIdAsync(tenantId))
                .ReturnsAsync((AppTenant?)null);

            // Act
            var result = await _service.RegisterAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.True(result.NotFound);
            Assert.Contains("Tenant not found.", result.Errors);

            _userRepository.VerifyNoOtherCalls();
            _jwtTokenService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task RegisterAsync_WhenEmailAlreadyRegistered_ReturnsConflict()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var request = CreateRequest(tenantId);

            SetupTenantExists(tenantId);
            SetupEmailRegistered(true);

            // Act
            var result = await _service.RegisterAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.True(result.Conflict);
            Assert.False(result.NotFound);
            Assert.Contains("Email is already registered.", result.Errors);

            _userRepository.Verify(r => r.IsEmailRegisteredAsync(Email), Times.Once);
            _userRepository.VerifyNoOtherCalls();
            _jwtTokenService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task RegisterAsync_WhenIdentityRejectsPassword_ReturnsErrorsWithoutToken()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var request = CreateRequest(tenantId);
            var identityError = "Passwords must have at least one digit ('0'-'9').";

            SetupTenantExists(tenantId);
            SetupEmailRegistered(false);

            _userRepository
                .Setup(r => r.CreateUserAsync(
                    Email,
                    string.Empty,
                    string.Empty,
                    Email,
                    Password,
                    tenantId))
                .ReturnsAsync(UserCreationResult.Failed(new[] { identityError }));

            // Act
            var result = await _service.RegisterAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.False(result.NotFound);
            Assert.False(result.Conflict);
            Assert.Contains(identityError, result.Errors);
            Assert.Null(result.Token);

            _jwtTokenService.VerifyNoOtherCalls();
        }

        private static RegisterRequest CreateRequest(Guid tenantId)
        {
            return new RegisterRequest
            {
                Email = Email,
                Password = Password,
                ConfirmPassword = Password,
                TenantId = tenantId
            };
        }

        private void SetupTenantExists(Guid tenantId)
        {
            _tenantRepository
                .Setup(r => r.GetByIdAsync(tenantId))
                .ReturnsAsync(new AppTenant { Id = tenantId, Name = TenantName });
        }

        private void SetupEmailRegistered(bool registered)
        {
            _userRepository
                .Setup(r => r.IsEmailRegisteredAsync(Email))
                .ReturnsAsync(registered);
        }

        private void VerifyNoDependencyCalls()
        {
            _userRepository.VerifyNoOtherCalls();
            _tenantRepository.VerifyNoOtherCalls();
            _jwtTokenService.VerifyNoOtherCalls();
        }
    }
}
