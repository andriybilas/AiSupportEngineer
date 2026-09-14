using AiSupport.Application.Models;
using AiSupport.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiSupport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TenantController : ControllerBase
    {
        private readonly TenantService _tenantService;

        public TenantController(TenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _tenantService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _tenantService.GetByIdAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
        {
            var result = await _tenantService.CreateAsync(request);

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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantRequest request)
        {
            var result = await _tenantService.UpdateAsync(id, request);

            if (result.NotFound)
            {
                return NotFound();
            }

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var updated = await _tenantService.GetByIdAsync(id);

            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _tenantService.DeleteAsync(id);

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

        [HttpPost("{tenantId:guid}/users/{userId:guid}")]
        public async Task<IActionResult> AddUser(Guid tenantId, Guid userId)
        {
            var result = await _tenantService.AddUserAsync(tenantId, userId);

            if (result.NotFound)
            {
                return NotFound(result.Errors);
            }

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        [HttpDelete("{tenantId:guid}/users/{userId:guid}")]
        public async Task<IActionResult> RemoveUser(Guid tenantId, Guid userId)
        {
            var result = await _tenantService.RemoveUserAsync(tenantId, userId);

            if (result.NotFound)
            {
                return NotFound(result.Errors);
            }

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
    }
}
