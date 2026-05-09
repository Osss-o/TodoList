using Application.Dtos.Todo;
using Application.Services.Interface;
using Domain.Constants;
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

      
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] TodoCreateDto todoDto)
        {
           

            try
            {
                await _todoService.CreateAsync(todoDto);

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
        public async Task<IActionResult> Update(int id, [FromForm] TodoUpdateDto todoUpdateDto)
        {

            try
            {
                await _todoService.UpdateAsync(id, todoUpdateDto);
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
                await _todoService.DeleteAsync(id);
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

            var todo = await _todoService.GetByIdAsync(id);
            if (todo == null)
            {
                return NotFound(new { message = $"Todo with ID {id} not found" });
            }
            return Ok(todo);
        }
        [HttpGet("GetQuery")]
        public async Task<IActionResult>GetQuery([FromQuery] TodoFilterDto filter)
        {
            var todos = await _todoService.GetQueryAsync(filter);
            return Ok(todos);
        }
        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] TodoFilterDto filter)
        {
            var result = await _todoService.SearchAsync(filter);
            return Ok(result);
        }
    }
}