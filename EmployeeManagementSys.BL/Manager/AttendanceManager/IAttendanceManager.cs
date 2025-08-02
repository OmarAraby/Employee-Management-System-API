

using EmployeeManagementSys.DL;

namespace EmployeeManagementSys.BL
{
    public interface IAttendanceManager
    {

        Task<APIResult<CheckInResponseDto>> CheckInAsync(CheckInDto checkInDto, string userRole);
        Task<APIResult<PagedList<AttendanceListDto>>> GetPaginatedAttendanceAsync(AttendanceQueryParams queryParams, string userRole);
        Task<APIResult<IEnumerable<AttendanceListDto>>> GetWeeklyAttendanceAsync(Guid employeeId, string userRole);
        Task<APIResult<IEnumerable<AttendanceListDto>>> GetDailyAttendanceListAsync(string userRole);
        //Task<APIResult<Dictionary<Guid, double>>> GetWeeklyWorkingHoursAsync(string userRole);
        Task<APIResult<IEnumerable<AttendanceListDto>>> GetMonthlyAttendanceAsync(Guid employeeId, int year, int month, string userRole);
    }

}
