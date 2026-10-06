using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;
using AiSupport.Domain.Models;

namespace AiSupport.Application.Services
{
    public class PaymentAttemptService
    {
        private readonly IPaymentAttemptRepository _paymentAttemptRepository;
        private readonly ICustomerRepository _customerRepository;

        public PaymentAttemptService(
            IPaymentAttemptRepository paymentAttemptRepository,
            ICustomerRepository customerRepository)
        {
            _paymentAttemptRepository = paymentAttemptRepository;
            _customerRepository = customerRepository;
        }

        public async Task<PaymentAttemptModel?> GetByIdAsync(Guid id)
        {
            var paymentAttempt = await _paymentAttemptRepository.GetByIdAsync(id);

            if (paymentAttempt == null)
            {
                return null;
            }

            return Map(paymentAttempt);
        }

        /// <summary>
        /// Returns payment attempts of a customer of the caller's tenant, newest first.
        /// Both bounds are inclusive timestamps: CreatedDateTime &gt;= from and CreatedDateTime &lt;= to.
        /// Input dates are normalized to UTC (Unspecified is treated as UTC, Local is converted).
        /// A customer of another tenant is reported as not found.
        /// </summary>
        public async Task<CustomerPaymentAttemptsResult> GetCustomerPaymentAttemptsAsync(
            Guid customerId,
            Guid tenantId,
            DateTime? from,
            DateTime? to)
        {
            var errors = new List<string>();

            if (customerId == Guid.Empty)
            {
                errors.Add("CustomerId is required.");
            }

            if (tenantId == Guid.Empty)
            {
                errors.Add("TenantId is required.");
            }

            var fromUtc = NormalizeToUtc(from);
            var toUtc = NormalizeToUtc(to);

            if (IsInvalidPeriod(fromUtc, toUtc))
            {
                errors.Add("'from' must be earlier than or equal to 'to'.");
            }

            if (errors.Count > 0)
            {
                return CustomerPaymentAttemptsResult.Failed(errors);
            }

            var customer = await _customerRepository.GetByIdAsync(customerId, tenantId);

            if (customer == null)
            {
                return CustomerPaymentAttemptsResult.NotFoundResult("Customer not found.");
            }

            var paymentAttempts = await _paymentAttemptRepository.GetByCustomerIdAndPeriodAsync(
                customer.Id,
                fromUtc,
                toUtc);

            var items = paymentAttempts
                .Select(Map)
                .ToList();

            return CustomerPaymentAttemptsResult.Ok(items);
        }

        private static bool IsInvalidPeriod(DateTime? fromUtc, DateTime? toUtc)
        {
            return fromUtc.HasValue
                && toUtc.HasValue
                && fromUtc.Value > toUtc.Value;
        }

        private static DateTime? NormalizeToUtc(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            var dateTime = value.Value;

            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return dateTime;
            }

            if (dateTime.Kind == DateTimeKind.Local)
            {
                return dateTime.ToUniversalTime();
            }

            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        private static PaymentAttemptModel Map(PaymentAttempt paymentAttempt)
        {
            return new PaymentAttemptModel
            {
                Id = paymentAttempt.Id,
                CustomerId = paymentAttempt.CustomerId,
                ProviderTransactionId = paymentAttempt.ProviderTransactionId,
                Amount = paymentAttempt.Amount,
                Currency = paymentAttempt.Currency,
                Status = paymentAttempt.Status.ToString(),
                ErrorCode = paymentAttempt.ErrorCode,
                CreatedDateTime = paymentAttempt.CreatedDateTime
            };
        }
    }
}
