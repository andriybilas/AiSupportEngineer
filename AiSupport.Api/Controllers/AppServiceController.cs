using AiSupport.Application.Models;
using AiSupport.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiSupport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppServiceController : ControllerBase
    {
        private readonly AppServiceCatalogService _appServiceCatalogService;

        public AppServiceController(AppServiceCatalogService appServiceCatalogService)
        {
            _appServiceCatalogService = appServiceCatalogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _appServiceCatalogService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _appServiceCatalogService.GetByIdAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAppServiceRequest request)
        {
            var result = await _appServiceCatalogService.CreateAsync(request);

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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppServiceRequest request)
        {
            var result = await _appServiceCatalogService.UpdateAsync(id, request);

            if (result.NotFound)
            {
                return NotFound();
            }

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var updated = await _appServiceCatalogService.GetByIdAsync(id);

            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _appServiceCatalogService.DeleteAsync(id);

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
