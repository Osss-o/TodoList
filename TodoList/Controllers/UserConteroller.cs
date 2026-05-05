using Application.Dtos.User;
using Application.Services.Interface;
using Domain.Constants;
using Domain.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace TodoList.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        private int CurrentuserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));


        [AllowAnonymous]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] UserCreateDto userDto)
        {
            try
            {
                await _userService.CreateAsync(userDto);
                return Ok(new { message = "The user was successfully created." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during creation.", detalils = ex.Message });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto userUpdateDto)
        {
            var isAdmin = User.IsInRole(RolesConst.ADMIN_ROLE) || User.IsInRole(RolesConst.SUPER_ADMIN_ROLE);

            if (!isAdmin && CurrentuserId != id)
                return Forbid("You cannot update another user's data.");

            try
            {
                await _userService.UpdateAsync(userUpdateDto, id, CurrentuserId, isAdmin);
                return Ok(new { message = " The user was successfully updated." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during the update", details = ex.Message });
            }
        }

        [Authorize(Roles = $"{RolesConst.SUPER_ADMIN_ROLE},{RolesConst.ADMIN_ROLE}")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var isAdmin = User.IsInRole(RolesConst.ADMIN_ROLE) || User.IsInRole(RolesConst.SUPER_ADMIN_ROLE);
            try
            {

                await _userService.DeleteAsync(id, CurrentuserId, isAdmin);
                return Ok(new { message = "The user has been deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during deletion.", details = ex.Message });
            }
        }

        [Authorize(Roles = $"{RolesConst.SUPER_ADMIN_ROLE},{RolesConst.ADMIN_ROLE}")]
        [HttpPatch("PromoteToAdmin/{id}")]
        public async Task<IActionResult> PromoteToAdmin(int id)
        {
            try
            {
                await _userService.PromoteToAdminAsync(id);
                return Ok(new { message = "User has been successfully promoted to Admin." });

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = $"{RolesConst.SUPER_ADMIN_ROLE},{RolesConst.ADMIN_ROLE}")]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] UserFilterDto filter)
        {
            var users = await _userService.GetAllAsync(filter);
            return Ok(users);
        }

        [Authorize(Roles = RolesConst.SUPER_ADMIN_ROLE)]
        [HttpPatch("DemoteFromAdmin/{id}")]
        public async Task<IActionResult> DemoteFromAdmin(int id)
        {
            try
            {
                var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (!Enum.TryParse(roleClaim, out RoleEnum role))
                {
                    return Unauthorized(new { message = "Invalid role claim." });
                }

                await _userService.DemoteFromAdminAsync(id, role);
                return Ok(new { message = "User has been successfully demoted from Admin." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}


