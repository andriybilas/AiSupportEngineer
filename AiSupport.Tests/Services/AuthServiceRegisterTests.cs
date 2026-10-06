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

        private readonly Mock<IUserRepository> _userRepository = new(MockBehavior.Strict);
        private readonly Mock<ITenantRepository> _tenantRepository = new(MockBehavior.Strict);
        private readonly Mock<ITransactionManager> _transactionManager = new(MockBehavior.Strict);
        private readonly Mock<IApplicationTransaction> _transaction = new(MockBehavior.Strict);
        private readonly Mock<IJwtTokenService> _jwtTokenService = new(MockBehavior.Strict);
        private readonly AuthService _service;

        public AuthServiceRegisterTests()
        {
            _transaction
                .Setup(t => t.CommitAsync())
                .Returns(Task.CompletedTask);

            _transaction
                .Setup(t => t.RollbackAsync())
                .Returns(Task.CompletedTask);

            _transaction
                .Setup(t => t.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            _transactionManager
                .Setup(m => m.BeginTransactionAsync())
                .ReturnsAsync(_transaction.Object);

            _service = new AuthService(
                _userRepository.Object,
                _tenantRepository.Object,
                _transactionManager.Object,
                _jwtTokenService.Object);
        }

        [Fact]
        public async Task RegisterAsync_WhenRequestIsValid_CreatesUserLinksTenantAndCommits()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var expiresAt = new DateTime(2026, 9, 29, 10, 0, 0, DateTimeKind.Utc);
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
                    Password))
                .ReturnsAsync(UserCreationResult.Ok(userId));

            _tenantRepository
                .Setup(r => r.AddUserAsync(tenantId, userId))
                .ReturnsAsync(EntityOperationResult.Ok());

            _jwtTokenService
                .Setup(s => s.CreateToken(userId, Email))
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
            Assert.Equal("jwt-token", result.Token);
            Assert.Equal(expiresAt, result.ExpiresAt);

            _userRepository.Verify(
                r => r.CreateUserAsync(
                    Email,
                    string.Empty,
                    string.Empty,
                    Email,
                    Password),
                Times.Once);

            _tenantRepository.Verify(
                r => r.AddUserAsync(tenantId, userId),
                Times.Once);

            _transaction.Verify(t => t.CommitAsync(), Times.Once);
            _transaction.Verify(t => t.RollbackAsync(), Times.Never);
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

            VerifyNoRepositoryCalls();
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

            VerifyNoRepositoryCalls();
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

            VerifyNoRepositoryCalls();
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
            _transactionManager.VerifyNoOtherCalls();
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
            _transactionManager.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task RegisterAsync_WhenIdentityRejectsPassword_ReturnsErrorsAndRollsBack()
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
                    Password))
                .ReturnsAsync(UserCreationResult.Failed(new[] { identityError }));

            // Act
            var result = await _service.RegisterAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.False(result.NotFound);
            Assert.False(result.Conflict);
            Assert.Contains(identityError, result.Errors);

            _tenantRepository.Verify(
                r => r.AddUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
                Times.Never);

            _transaction.Verify(t => t.RollbackAsync(), Times.Once);
            _transaction.Verify(t => t.CommitAsync(), Times.Never);
            _jwtTokenService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task RegisterAsync_WhenLinkingToTenantFails_RollsBackAndReturnsFailure()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = CreateRequest(tenantId);

            SetupTenantExists(tenantId);
            SetupEmailRegistered(false);

            _userRepository
                .Setup(r => r.CreateUserAsync(
                    Email,
                    string.Empty,
                    string.Empty,
                    Email,
                    Password))
                .ReturnsAsync(UserCreationResult.Ok(userId));

            _tenantRepository
                .Setup(r => r.AddUserAsync(tenantId, userId))
                .ReturnsAsync(EntityOperationResult.Failed("User is already a member of this tenant."));

            // Act
            var result = await _service.RegisterAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("User is already a member of this tenant.", result.Errors);

            _transaction.Verify(t => t.RollbackAsync(), Times.Once);
            _transaction.Verify(t => t.CommitAsync(), Times.Never);
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
                .ReturnsAsync(new AppTenant { Id = tenantId, Name = "Acme" });
        }

        private void SetupEmailRegistered(bool registered)
        {
            _userRepository
                .Setup(r => r.IsEmailRegisteredAsync(Email))
                .ReturnsAsync(registered);
        }

        private void VerifyNoRepositoryCalls()
        {
            _userRepository.VerifyNoOtherCalls();
            _tenantRepository.VerifyNoOtherCalls();
            _transactionManager.VerifyNoOtherCalls();
            _jwtTokenService.VerifyNoOtherCalls();
        }
    }
}
