using EmployeeManagementSys.BL;
using EmployeeManagementSys.DL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeManagementSys.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeManager _employeeManager;

        public EmployeeController(IEmployeeManager employeeManager)
        {
            _employeeManager = employeeManager;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddEmployee([FromBody] CreateEmployeeDto createDto)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var result = await _employeeManager.AddEmployeeAsync(createDto, userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] EmployeeDto updateDto)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            updateDto.Id = id;
            var result = await _employeeManager.UpdateEmployeeAsync(updateDto, userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var result = await _employeeManager.DeleteEmployeeAsync(id, userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPaginatedEmployees([FromQuery] EmployeeQueryParams queryParams)
        {
            var result = await _employeeManager.GetPaginatedEmployeesAsync(queryParams);
            return Ok(result);
        }

        [HttpGet("profile/{employeeId}")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetEmployeeProfile(Guid employeeId)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var result = await _employeeManager.GetEmployeeProfileAsync(employeeId, userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
