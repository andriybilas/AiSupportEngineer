using AiSupport.Application.Services;
using AiSupport.Infrastructure.Auth;
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

        /// <summary>
        /// Returns payment attempts of a customer of the caller's tenant, newest first.
        /// Optional 'from' and 'to' are inclusive timestamp bounds on CreatedDateTime.
        /// Values without an offset are treated as UTC.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get(
            Guid customerId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            // A valid token without a usable tenant claim (e.g. issued before the claim existed)
            // is treated as unusable credentials: 401 tells the client to log in again.
            if (!TryGetTenantId(out var tenantId))
            {
                return Unauthorized();
            }

            var result = await _paymentAttemptService.GetCustomerPaymentAttemptsAsync(
                customerId,
                tenantId,
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

        private bool TryGetTenantId(out Guid tenantId)
        {
            var tenantClaim = User.FindFirst(AiSupportClaims.TenantId);

            if (tenantClaim == null)
            {
                tenantId = Guid.Empty;
                return false;
            }

            return Guid.TryParse(tenantClaim.Value, out tenantId)
                && tenantId != Guid.Empty;
        }
    }
}
