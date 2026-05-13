using Application.Dtos.FileAttachment;
using Application.Services.Interface;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace TodoList.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FileAttachmentController : ControllerBase
    {
        private readonly IFileAttachmentService _fileService;
        private readonly IWebHostEnvironment _env;
        private readonly ICurrentUserService _currentUserService;
        public FileAttachmentController(IFileAttachmentService fileService, IWebHostEnvironment env, ICurrentUserService currentUserService)
        {
            _fileService = fileService;
            _env = env;
            _currentUserService = currentUserService;
        }
       
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm]FileAttachmentCreateDto fileAttachment)
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

        [HttpPost("CreateMany")]
        public async Task<IActionResult> CreateMany([FromForm] int todoId, [FromForm] List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                    return BadRequest(new { Message = "Please select at least one file." });

                await _fileService.CreateManyAsync(files, todoId);
                return Ok(new { Message = $"{files.Count} files uploaded successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { Message = "An error occurred during multi-file upload." });
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

        [HttpGet("Download/{id}")]
        public async Task<IActionResult> Dwonload(int id)
        {
            var fileAttachmentD = await _fileService.GetByIdAsync(id);

            if (fileAttachmentD == null)
                return NotFound(new { Message = $"File attachment with ID {id} not found" });

            var filePath = Path.Combine(_env.WebRootPath, fileAttachmentD.FilePath.Replace("/", "\\"));

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { Message = "File not found on srever. " });

            var contentType = fileAttachmentD.ContentType ?? "application/octet-stream";
          
            return File(await System.IO.File.ReadAllBytesAsync(filePath), contentType, fileAttachmentD.FileName);
        }
    }
}