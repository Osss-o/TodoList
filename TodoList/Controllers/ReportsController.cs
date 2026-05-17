using Application.Dtos.Reports;
using Application.Services.Interface;
using Domain.Constants;
using Domain.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TodoList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }
        [Authorize(Roles =$"{RolesConst.SUPER_ADMIN_ROLE},{RolesConst.ADMIN_ROLE}")]
        [HttpGet("user-productivity")]
        public async Task<IActionResult> GetUserProductivity([FromQuery] UserProductivityFilterDto filter)
        {
            try
            {
                var report = await _reportService.GetUserProductivityReportAsync(filter);
                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating the report.", details = ex.Message });
            }
           
        }
        [Authorize(Roles = $"{RolesConst.SUPER_ADMIN_ROLE},{RolesConst.ADMIN_ROLE}")]
        [HttpGet("category-usage")]
        public async Task<IActionResult> GetCategoryUsage([FromQuery] CategoryUsageFilterDto filter)
        {
            try
            {
                var report = await _reportService.GetCategoryUsageReportAsync(filter);
                return Ok(report);

            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating the report.", details = ex.Message });

            }  
        }
    }
}
