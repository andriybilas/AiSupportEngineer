using AiSupport.Application.Models;
using AiSupport.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiSupport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppUserService _userService;

        public UserController(AppUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            var result = await _userService.RegisterAsync(request);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return CreatedAtAction(
                nameof(GetUser),
                new { userId = result.UserId },
                new { userId = result.UserId });
        }

        [Authorize]
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUser(Guid userId)
        {
            var user = await _userService.GetUserById(userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [Authorize]
        [HttpPut("{userId:guid}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserRequest request)
        {
            var result = await _userService.UpdateUserAsync(userId, request);

            if (result.UserNotFound)
            {
                return NotFound();
            }

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var updatedUser = await _userService.GetUserById(userId);

            if (updatedUser == null)
            {
                return NotFound();
            }

            return Ok(updatedUser);
        }
    }
}
