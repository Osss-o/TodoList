using Application.Dtos.Auth;
using Application.Services.Interface;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TodoList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto input)
        {
            var result = await _authService.LoginAsync(input);
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }
            return Ok(result);
        }


        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
           var result = await _authService.RefreshToken(refreshToken);
          
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }
            return Ok(result);
        }


        [Authorize(Roles = RolesConst.USER_ROLE)]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto input)
        {
            try
            {
                await _authService.ChangePassword(input);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }


        [Authorize(Roles = $"{RolesConst.SUPER_ADMIN_ROLE},{RolesConst.ADMIN_ROLE}")]
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(int userId, string newpassword)
        {
            try
            {
                await _authService.ResetPassword(userId, newpassword);
                return Ok(new { message = "Password reset successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

