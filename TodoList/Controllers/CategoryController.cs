using Application.Dtos.Category;
using Application.Services;
using Application.Services.Interface;
using Domain.Constants;
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
        private readonly ICurrentUserService _currentUserService;

        public CategoryController(ICategoryService categoryService, ICurrentUserService currentUserService)
        {
            _categoryService = categoryService;
            _currentUserService = currentUserService;
        }

        [Authorize]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto categoryDto)
        {
            try
            {

                await _categoryService.CreateAsync(categoryDto, _currentUserService.UserId);
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
                await _categoryService.DeleteAsync(id, _currentUserService.UserId,_currentUserService.IsAdmin, deleteLinkedTodos);
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
                await _categoryService.UpdateAsync(id,_currentUserService.UserId, categoryDto);
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

            var categories = await _categoryService.GetAllAsync(filter, _currentUserService.UserId, _currentUserService.IsAdmin);
            return Ok(categories);
        }

        [Authorize]
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id, _currentUserService.UserId);
            if (category == null)
            {
                return NotFound(new { message = $"Category with ID {id} not found." });
            }
            return Ok(category);
        }
    }
}
