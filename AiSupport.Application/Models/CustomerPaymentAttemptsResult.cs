namespace AiSupport.Application.Models
{
    public class CustomerPaymentAttemptsResult
    {
        public bool Succeeded { get; set; }
        public bool NotFound { get; set; }
        public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();
        public IReadOnlyList<PaymentAttemptModel> Items { get; set; } = Array.Empty<PaymentAttemptModel>();

        public static CustomerPaymentAttemptsResult Ok(IEnumerable<PaymentAttemptModel> items)
        {
            return new CustomerPaymentAttemptsResult
            {
                Succeeded = true,
                NotFound = false,
                Errors = Array.Empty<string>(),
                Items = items.ToList()
            };
        }

        public static CustomerPaymentAttemptsResult NotFoundResult(string message = "Customer not found.")
        {
            return new CustomerPaymentAttemptsResult
            {
                Succeeded = false,
                NotFound = true,
                Errors = new[] { message },
                Items = Array.Empty<PaymentAttemptModel>()
            };
        }

        public static CustomerPaymentAttemptsResult Failed(IEnumerable<string> errors)
        {
            return new CustomerPaymentAttemptsResult
            {
                Succeeded = false,
                NotFound = false,
                Errors = errors.ToList(),
                Items = Array.Empty<PaymentAttemptModel>()
            };
        }

        public static CustomerPaymentAttemptsResult Failed(params string[] errors)
        {
            return Failed((IEnumerable<string>)errors);
        }
    }
}
