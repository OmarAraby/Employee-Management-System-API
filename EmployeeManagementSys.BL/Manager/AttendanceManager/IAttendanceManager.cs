

using EmployeeManagementSys.DL;

namespace EmployeeManagementSys.BL
{
    public interface IAttendanceManager
    {

        Task<APIResult<CheckInResponseDto>> CheckInAsync(CheckInDto checkInDto, string userRole, Guid callerId);
        Task<APIResult<CheckInResponseDto>> CheckOutAsync(string userRole, Guid callerId);
        Task<APIResult<PagedList<AttendanceListDto>>> GetPaginatedAttendanceAsync(AttendanceQueryParams queryParams, string userRole);
        Task<APIResult<IEnumerable<AttendanceListDto>>> GetWeeklyAttendanceAsync(Guid employeeId, string userRole, Guid callerId);
        Task<APIResult<IEnumerable<AttendanceListDto>>> GetDailyAttendanceListAsync(string userRole);
        //Task<APIResult<Dictionary<Guid, double>>> GetWeeklyWorkingHoursAsync(string userRole);
        Task<APIResult<IEnumerable<AttendanceListDto>>> GetMonthlyAttendanceAsync(Guid employeeId, int year, int month, string userRole, Guid callerId);
        Task<APIResult<byte[]>> GetMonthlyAttendanceReportCsvAsync(Guid employeeId, int year, int month, string userRole, Guid callerId);
    }

}
