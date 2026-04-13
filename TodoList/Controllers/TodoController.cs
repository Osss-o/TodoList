using Application.Dtos.Todo;
using Application.Services.Interface;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TodoList.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodoController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        private bool IsAdmin => User.IsInRole(TodoConst.ADMIN_ROLE);
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));


        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] TodoCreateDto todoDto)
        {
            if (IsAdmin)
            {
                return StatusCode(403, new {
                    message = "Admins are restricted from creating personal tasks."
                });
            }

            try
            {
                await _todoService.CreateAsync(todoDto, CurrentUserId);

                return Ok(new { message = "Todo created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the todo", details = ex.Message });
            }
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TodoUpdateDto todoUpdateDto)
        {

            try
            {
                await _todoService.UpdateAsync(id, todoUpdateDto, CurrentUserId, IsAdmin);
                return Ok(new { message = "Todo updated successfully" });
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
                return StatusCode(500, new { message = "An error occurred while updating the todo", details = ex.Message });
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {

            try
            {
                await _todoService.DeleteAsync(id, CurrentUserId, IsAdmin);
                return Ok(new { message = "Todo deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the todo", details = ex.Message });
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {

            var todo = await _todoService.GetByIdAsync(id, CurrentUserId);
            if (todo == null)
            {
                return NotFound(new { message = $"Todo with ID {id} not found" });
            }
            return Ok(todo);
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] TodoFilterDto filter)
        {
            var todos = await _todoService.GetAllAsync(filter, CurrentUserId);
            return Ok(todos);
        }
        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] TodoFilterDto filter)
        {
            var isAdmin = User.IsInRole(TodoConst.ADMIN_ROLE);
            var result = await _todoService.SearchAsync(filter, CurrentUserId, isAdmin);
            return Ok(result);
        }
    }
}