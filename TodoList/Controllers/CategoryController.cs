using Application.Dtos.Category;
using Application.Services.Interface;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TodoList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        private bool IsAdmin => User.IsInRole(TodoConst.ADMIN_ROLE);

        [Authorize]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto categoryDto)
        {
            try
            {

                await _categoryService.CreateAsync(categoryDto, CurrentUserId);
                return Ok(new { message = "Category created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] bool deleteLinkedTodos = false)
        {
            try
            {
                await _categoryService.DeleteAsync(id,CurrentUserId,IsAdmin,deleteLinkedTodos);
                return Ok(new { message = "Category deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [Authorize]
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto categoryDto)
        {
            try
            {
                await _categoryService.UpdateAsync(id,CurrentUserId, categoryDto );
                return Ok(new { message = "Category updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetALl([FromQuery] CategoryFilterDto filter)
        {
            var categories = await _categoryService.GetAllAsync(filter, CurrentUserId);
            return Ok(categories);
        }

        [Authorize]
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id, CurrentUserId);
            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }
            return Ok(category);
        }
    }
}
