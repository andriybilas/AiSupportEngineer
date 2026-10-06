using AiSupport.Application.Models;
using AiSupport.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiSupport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Succeeded)
            {
                return Unauthorized(result.Errors);
            }

            return Ok(new
            {
                token = result.Token,
                expiresAt = result.ExpiresAt,
                userId = result.UserId,
                userName = result.UserName
            });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (result.NotFound)
            {
                return NotFound(result.Errors);
            }

            if (result.Conflict)
            {
                return Conflict(result.Errors);
            }

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var response = new
            {
                userId = result.UserId,
                email = result.Email,
                tenantId = result.TenantId,
                tenantName = result.TenantName,
                token = result.Token,
                expiresAt = result.ExpiresAt
            };

            return CreatedAtAction(
                nameof(UserController.GetUser),
                "User",
                new { userId = result.UserId },
                response);
        }
    }
}
