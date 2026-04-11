using Application.Dtos.FileAttachment;
using Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TodoList.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FileAttachmentController : ControllerBase
    {
        private readonly IFileAttachmentService _fileService;

        public FileAttachmentController(IFileAttachmentService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] FileAttachmentCreateDto fileAttachment)
        {
            try
            {
                await _fileService.CreateAsync(fileAttachment);
                return Ok(new { Message = "File attachment created successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { Message = "An error occurred while creating the file attachment." });
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _fileService.DeleteAsync(id);
                return Ok(new { Message = "File attachment deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = $"File attachment with ID {id} not found." });
            }
            catch (IOException ex)

            {
                return NotFound(new { Message = $"File not found on server: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { massage = "An error occurred while delete the file attachment." });
            }

        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FileAttachmentUpdateDto input)
        {
            try
            {
                await _fileService.UpdateAsync(id, input);
                return Ok(new { Message = "File attachment updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = $"File attachment with ID {id} not found." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the file attachment." });
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var fileAttachment = await _fileService.GetByIdAsync(id);
                if (fileAttachment == null)
                {
                    return NotFound(new { Message = $"File attachment with ID {id} not found." });
                }
                return Ok(fileAttachment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving the file attachment." });
            }
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] FileAttachmentFilterDto filter)
        {
            try
            {
                var fileAttachments = await _fileService.GetAllAsync(filter);
                return Ok(fileAttachments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving file attachments." });
            }
        }

    }
}
