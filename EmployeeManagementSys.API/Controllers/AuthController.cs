using EmployeeManagementSys.BL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSys.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationManager _authManager;
        public AuthController(IAuthenticationManager authManager)
        {
            _authManager = authManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _authManager.LoginAsync(loginDto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("reset-password")]
        [Authorize]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetDto)
        {
            var result = await _authManager.ResetPasswordAsync(resetDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotDto)
        {
            // Always 200 (enumeration-safe) — the manager sends the email only
            // if the account exists, but never reveals that to the caller.
            var result = await _authManager.ForgotPasswordAsync(forgotDto);
            return Ok(result);
        }

        [HttpPost("reset-password-confirm")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordConfirm([FromBody] ResetPasswordWithTokenDto resetDto)
        {
            var result = await _authManager.ResetPasswordWithTokenAsync(resetDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

       



    }
}
