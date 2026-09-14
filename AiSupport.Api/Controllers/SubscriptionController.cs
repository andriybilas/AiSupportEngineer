using AiSupport.Application.Models;
using AiSupport.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiSupport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionController(SubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? tenantId = null)
        {
            var items = await _subscriptionService.GetAllAsync(tenantId);
            return Ok(items);
        }

        [HttpGet("tenant/{tenantId:guid}")]
        public async Task<IActionResult> GetByTenant(Guid tenantId)
        {
            var items = await _subscriptionService.GetAllAsync(tenantId);
            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _subscriptionService.GetByIdAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSubscriptionRequest request)
        {
            var result = await _subscriptionService.CreateAsync(request);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Entity!.Id },
                result.Entity);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubscriptionRequest request)
        {
            var result = await _subscriptionService.UpdateAsync(id, request);

            if (result.NotFound)
            {
                return NotFound();
            }

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var updated = await _subscriptionService.GetByIdAsync(id);

            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _subscriptionService.DeleteAsync(id);

            if (result.NotFound)
            {
                return NotFound();
            }

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
    }
}
