using AiSupport.Application.Abstractions;
using AiSupport.Application.Services;
using AiSupport.Domain.Models;
using Moq;

namespace AiSupport.Tests.Services
{
    public class PaymentAttemptServiceTests
    {
        private readonly Mock<IPaymentAttemptRepository> _paymentAttemptRepository = new(MockBehavior.Strict);
        private readonly Mock<ICustomerRepository> _customerRepository = new(MockBehavior.Strict);
        private readonly PaymentAttemptService _service;

        public PaymentAttemptServiceTests()
        {
            _service = new PaymentAttemptService(
                _paymentAttemptRepository.Object,
                _customerRepository.Object);
        }

        [Fact]
        public async Task GetCustomerPaymentAttemptsAsync_WhenCustomerBelongsToTenant_ReturnsMappedAttemptsForPeriod()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var customer = CreateCustomer(tenantId);
            var from = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Unspecified);
            var to = new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc);

            var completed = new PaymentAttempt
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                ProviderTransactionId = "txn_2",
                Amount = 49.99m,
                Currency = "USD",
                Status = PaymentStatus.Completed,
                ErrorCode = null,
                CreatedDateTime = new DateTime(2026, 9, 20, 10, 0, 0, DateTimeKind.Utc)
            };

            var failed = new PaymentAttempt
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                ProviderTransactionId = "txn_1",
                Amount = 49.99m,
                Currency = "USD",
                Status = PaymentStatus.Failed,
                ErrorCode = "card_declined",
                CreatedDateTime = new DateTime(2026, 9, 10, 8, 30, 0, DateTimeKind.Utc)
            };

            SetupCustomerLookup(customer.Id, tenantId, customer);

            _paymentAttemptRepository
                .Setup(r => r.GetByCustomerIdAndPeriodAsync(
                    customer.Id,
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(new List<PaymentAttempt> { completed, failed });

            // Act
            var result = await _service.GetCustomerPaymentAttemptsAsync(customer.Id, tenantId, from, to);

            // Assert
            Assert.True(result.Succeeded);
            Assert.False(result.NotFound);
            Assert.Empty(result.Errors);
            Assert.Equal(2, result.Items.Count);

            var first = result.Items[0];
            Assert.Equal(completed.Id, first.Id);
            Assert.Equal(customer.Id, first.CustomerId);
            Assert.Equal("txn_2", first.ProviderTransactionId);
            Assert.Equal(49.99m, first.Amount);
            Assert.Equal("USD", first.Currency);
            Assert.Equal("Completed", first.Status);
            Assert.Null(first.ErrorCode);
            Assert.Equal(completed.CreatedDateTime, first.CreatedDateTime);

            var second = result.Items[1];
            Assert.Equal(failed.Id, second.Id);
            Assert.Equal("Failed", second.Status);
            Assert.Equal("card_declined", second.ErrorCode);

            _paymentAttemptRepository.Verify(
                r => r.GetByCustomerIdAndPeriodAsync(
                    customer.Id,
                    It.Is<DateTime?>(d => IsUtcWithTicks(d, from.Ticks)),
                    It.Is<DateTime?>(d => IsUtcWithTicks(d, to.Ticks))),
                Times.Once);
        }

        [Fact]
        public async Task GetCustomerPaymentAttemptsAsync_PassesRequestedCustomerAndTenantToLookupAndCustomerIdToAttemptsQuery()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var requestedCustomerId = Guid.NewGuid();
            var customer = CreateCustomer(tenantId, requestedCustomerId);

            SetupCustomerLookup(requestedCustomerId, tenantId, customer);

            _paymentAttemptRepository
                .Setup(r => r.GetByCustomerIdAndPeriodAsync(customer.Id, null, null))
                .ReturnsAsync(new List<PaymentAttempt>());

            // Act
            var result = await _service.GetCustomerPaymentAttemptsAsync(requestedCustomerId, tenantId, null, null);

            // Assert
            Assert.True(result.Succeeded);

            _customerRepository.Verify(
                r => r.GetByIdAsync(requestedCustomerId, tenantId),
                Times.Once);

            _customerRepository.VerifyNoOtherCalls();

            _paymentAttemptRepository.Verify(
                r => r.GetByCustomerIdAndPeriodAsync(customer.Id, null, null),
                Times.Once);

            _paymentAttemptRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetCustomerPaymentAttemptsAsync_WhenCustomerBelongsToAnotherTenant_ReturnsNotFound()
        {
            // Arrange
            var callerTenantId = Guid.NewGuid();
            var foreignCustomerId = Guid.NewGuid();

            // The tenant-scoped lookup finds nothing for a customer owned by another tenant.
            SetupCustomerLookup(foreignCustomerId, callerTenantId, null);

            // Act
            var result = await _service.GetCustomerPaymentAttemptsAsync(foreignCustomerId, callerTenantId, null, null);

            // Assert
            Assert.False(result.Succeeded);
            Assert.True(result.NotFound);
            Assert.Empty(result.Items);
            Assert.Contains("Customer not found.", result.Errors);

            _customerRepository.Verify(
                r => r.GetByIdAsync(foreignCustomerId, callerTenantId),
                Times.Once);

            _paymentAttemptRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetCustomerPaymentAttemptsAsync_WhenDatesAreLocal_PassesDatesConvertedToUtc()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var customer = CreateCustomer(tenantId);
            var from = new DateTime(2026, 9, 1, 12, 0, 0, DateTimeKind.Local);
            var to = new DateTime(2026, 9, 2, 12, 0, 0, DateTimeKind.Local);
            var expectedFromTicks = from.ToUniversalTime().Ticks;
            var expectedToTicks = to.ToUniversalTime().Ticks;

            SetupCustomerLookup(customer.Id, tenantId, customer);

            _paymentAttemptRepository
                .Setup(r => r.GetByCustomerIdAndPeriodAsync(
                    customer.Id,
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(new List<PaymentAttempt>());

            // Act
            var result = await _service.GetCustomerPaymentAttemptsAsync(customer.Id, tenantId, from, to);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Empty(result.Items);

            _paymentAttemptRepository.Verify(
                r => r.GetByCustomerIdAndPeriodAsync(
                    customer.Id,
                    It.Is<DateTime?>(d => IsUtcWithTicks(d, expectedFromTicks)),
                    It.Is<DateTime?>(d => IsUtcWithTicks(d, expectedToTicks))),
                Times.Once);
        }

        [Fact]
        public async Task GetCustomerPaymentAttemptsAsync_WhenPeriodNotProvided_PassesNullBounds()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var customer = CreateCustomer(tenantId);

            SetupCustomerLookup(customer.Id, tenantId, customer);

            _paymentAttemptRepository
                .Setup(r => r.GetByCustomerIdAndPeriodAsync(customer.Id, null, null))
                .ReturnsAsync(new List<PaymentAttempt>());

            // Act
            var result = await _service.GetCustomerPaymentAttemptsAsync(customer.Id, tenantId, null, null);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Empty(result.Items);

            _paymentAttemptRepository.Verify(
                r => r.GetByCustomerIdAndPeriodAsync(customer.Id, null, null),
                Times.Once);
        }

        [Fact]
        public async Task GetCustomerPaymentAttemptsAsync_WhenCustomerMissing_ReturnsNotFound()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            SetupCustomerLookup(customerId, tenantId, null);

            // Act
            var result = await _service.GetCustomerPaymentAttemptsAsync(customerId, tenantId, null, null);

            // Assert
            Assert.False(result.Succeeded);
            Assert.True(result.NotFound);
            Assert.Empty(result.Items);
            Assert.Contains("Customer not found.", result.Errors);

            _paymentAttemptRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetCustomerPaymentAttemptsAsync_WhenFromIsLaterThanTo_ReturnsValidationFailure()
        {
            // Arrange
            var from = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc);
            var to = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = await _service.GetCustomerPaymentAttemptsAsync(Guid.NewGuid(), Guid.NewGuid(), from, to);

            // Assert
            Assert.False(result.Succeeded);
            Assert.False(result.NotFound);
            Assert.Empty(result.Items);
            Assert.Contains("'from' must be earlier than or equal to 'to'.", result.Errors);

            VerifyNoRepositoryCalls();
        }

        [Fact]
        public async Task GetCustomerPaymentAttemptsAsync_WhenCustomerIdIsEmpty_ReturnsValidationFailure()
        {
            // Act
            var result = await _service.GetCustomerPaymentAttemptsAsync(Guid.Empty, Guid.NewGuid(), null, null);

            // Assert
            Assert.False(result.Succeeded);
            Assert.False(result.NotFound);
            Assert.Empty(result.Items);
            Assert.Contains("CustomerId is required.", result.Errors);

            VerifyNoRepositoryCalls();
        }

        [Fact]
        public async Task GetCustomerPaymentAttemptsAsync_WhenTenantIdIsEmpty_ReturnsValidationFailure()
        {
            // Act
            var result = await _service.GetCustomerPaymentAttemptsAsync(Guid.NewGuid(), Guid.Empty, null, null);

            // Assert
            Assert.False(result.Succeeded);
            Assert.False(result.NotFound);
            Assert.Empty(result.Items);
            Assert.Contains("TenantId is required.", result.Errors);

            VerifyNoRepositoryCalls();
        }

        private void SetupCustomerLookup(Guid customerId, Guid tenantId, Customer? customer)
        {
            _customerRepository
                .Setup(r => r.GetByIdAsync(customerId, tenantId))
                .ReturnsAsync(customer);
        }

        private void VerifyNoRepositoryCalls()
        {
            _customerRepository.VerifyNoOtherCalls();
            _paymentAttemptRepository.VerifyNoOtherCalls();
        }

        private static Customer CreateCustomer(Guid tenantId, Guid? customerId = null)
        {
            return new Customer
            {
                Id = customerId ?? Guid.NewGuid(),
                TenantId = tenantId,
                Name = "Acme Corp",
                CreatedDateTime = DateTime.UtcNow,
                UpdatedDateTime = DateTime.UtcNow
            };
        }

        private static bool IsUtcWithTicks(DateTime? value, long expectedTicks)
        {
            return value.HasValue
                && value.Value.Kind == DateTimeKind.Utc
                && value.Value.Ticks == expectedTicks;
        }
    }
}
