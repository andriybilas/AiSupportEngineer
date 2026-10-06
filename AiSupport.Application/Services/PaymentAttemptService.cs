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

        public async Task<CustomerPaymentAttemptsResult> GetCustomerPaymentAttemptsAsync(
            Guid customerId,
            DateTime? from,
            DateTime? to)
        {
            if (customerId == Guid.Empty)
            {
                return CustomerPaymentAttemptsResult.Failed("CustomerId is required.");
            }

            var fromUtc = NormalizeToUtc(from);
            var toUtc = NormalizeToUtc(to);

            if (IsInvalidPeriod(fromUtc, toUtc))
            {
                return CustomerPaymentAttemptsResult.Failed("'from' must be earlier than or equal to 'to'.");
            }

            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer == null)
            {
                return CustomerPaymentAttemptsResult.NotFoundResult("Customer not found.");
            }

            var paymentAttempts = await _paymentAttemptRepository.GetByCustomerIdAndPeriodAsync(
                customerId,
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
