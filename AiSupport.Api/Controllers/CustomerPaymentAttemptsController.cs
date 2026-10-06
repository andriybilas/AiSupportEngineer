using AiSupport.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiSupport.Api.Controllers
{
    [ApiController]
    [Route("api/customers/{customerId:guid}/payment-attempts")]
    [Authorize]
    public class CustomerPaymentAttemptsController : ControllerBase
    {
        private readonly PaymentAttemptService _paymentAttemptService;

        public CustomerPaymentAttemptsController(PaymentAttemptService paymentAttemptService)
        {
            _paymentAttemptService = paymentAttemptService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            Guid customerId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var result = await _paymentAttemptService.GetCustomerPaymentAttemptsAsync(
                customerId,
                from,
                to);

            if (result.NotFound)
            {
                return NotFound(result.Errors);
            }

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Items);
        }
    }
}
