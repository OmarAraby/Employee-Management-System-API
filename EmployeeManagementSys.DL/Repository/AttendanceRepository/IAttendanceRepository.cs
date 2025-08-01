

namespace EmployeeManagementSys.DL;

public interface IAttendanceRepository
{
    Task<Attendance> GetByIdAsync(Guid id);
    Task<Attendance> AddAsync(Attendance attendance);
    Task<bool> DeleteAsync(Guid id);

    // daily attendance
    Task<bool> HasCheckedInTodayAsync(Guid employeeId);
    Task<Attendance?> GetTodayAttendanceAsync(Guid employeeId);
    Task<Attendance?> GetByEmployeeIdAndDateAsync(Guid employeeId, DateTime date);

    // paginated attendance
    Task<(PagedList<Attendance> Items, int TotalCount)> GetPaginatedAttendanceAsync(AttendanceQueryParams queryParams);




    // Dashboard statistics
    Task<int> GetTotalAttendanceCountAsync();
    Task<int> GetTodayAttendanceCountAsync();
    Task<int> GetMonthlyAttendanceCountAsync(int year, int month);



    // Weekly/Monthly reports
    Task<IEnumerable<Attendance>> GetWeeklyAttendanceAsync(Guid employeeId, DateTime weekStartDate);
    Task<IEnumerable<Attendance>> GetMonthlyAttendanceAsync(Guid employeeId, int year, int month);




}
