using EmployeeManagementSys.BL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeManagementSys.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceManager _attendanceManager;

        public AttendanceController(IAttendanceManager attendanceManager)
        {
            _attendanceManager = attendanceManager;
        }

        [HttpPost("check-in")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInDto checkInDto)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var result = await _attendanceManager.CheckInAsync(checkInDto, userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("weekly/{employeeId}")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetWeeklyAttendance(Guid employeeId)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var result = await _attendanceManager.GetWeeklyAttendanceAsync(employeeId, userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("daily")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDailyAttendance()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var result = await _attendanceManager.GetDailyAttendanceListAsync(userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("monthly/{employeeId}/{year}/{month}")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetMonthlyAttendance(Guid employeeId, int year, int month)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var result = await _attendanceManager.GetMonthlyAttendanceAsync(employeeId, year, month, userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("weekly-hours")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetWeeklyWorkingHours()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var result = await _attendanceManager.GetWeeklyWorkingHoursAsync(userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
