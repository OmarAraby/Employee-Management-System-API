using EmployeeManagementSys.API.Extensions;
using EmployeeManagementSys.BL;
using EmployeeManagementSys.DL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet()]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPaginatedAttendance([FromQuery] AttendanceQueryParams queryParams)
        {
            var userRole = User.GetRole();
            var result = await _attendanceManager.GetPaginatedAttendanceAsync(queryParams, userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("check-in")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInDto checkInDto)
        {
            var userRole = User.GetRole();
            if (!User.TryGetUserId(out var callerId)) return Forbid();
            var result = await _attendanceManager.CheckInAsync(checkInDto, userRole, callerId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("weekly/{employeeId}")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> GetWeeklyAttendance(Guid employeeId)
        {
            var userRole = User.GetRole();
            if (!User.TryGetUserId(out var callerId)) return Forbid();
            var result = await _attendanceManager.GetWeeklyAttendanceAsync(employeeId, userRole, callerId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("daily")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDailyAttendance()
        {
            var userRole = User.GetRole();
            var result = await _attendanceManager.GetDailyAttendanceListAsync(userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("monthly/{employeeId}")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> GetMonthlyAttendance(Guid employeeId, [FromQuery] int? year, [FromQuery] int? month)
        {
            var userRole = User.GetRole();
            if (!User.TryGetUserId(out var callerId)) return Forbid();
            if (!year.HasValue || !month.HasValue)
            {
                return BadRequest(new APIResult<IEnumerable<AttendanceListDto>>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "ValidationError", Message = "Year and month are required query parameters." } }
                });
            }
            var result = await _attendanceManager.GetMonthlyAttendanceAsync(employeeId, year.Value, month.Value, userRole, callerId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("monthly/{employeeId}/report")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> GetMonthlyAttendanceReport(Guid employeeId, [FromQuery] int? year, [FromQuery] int? month)
        {
            var userRole = User.GetRole();
            if (!User.TryGetUserId(out var callerId)) return Forbid();
            if (!year.HasValue || !month.HasValue || month.Value < 1 || month.Value > 12 || year.Value < 2000 || year.Value > 9999)
            {
                return BadRequest(new APIResult<byte[]>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "ValidationError", Message = "A valid year (2000-9999) and month (1-12) are required." } }
                });
            }
            var result = await _attendanceManager.GetMonthlyAttendanceReportCsvAsync(employeeId, year.Value, month.Value, userRole, callerId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            var fileName = $"attendance-{year.Value:D4}-{month.Value:D2}.csv";
            return File(result.Data!, "text/csv", fileName);
        }

        //[HttpGet("weekly-hours")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> GetWeeklyWorkingHours()
        //{
        //    var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        //    var result = await _attendanceManager.GetWeeklyWorkingHoursAsync(userRole);
        //    return result.Success ? Ok(result) : BadRequest(result);
        //}
    }
}
