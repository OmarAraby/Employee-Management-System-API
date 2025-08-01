using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagementSys.DL;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly EmployeeManagementSysDbContext _context;

    public AttendanceRepository(EmployeeManagementSysDbContext context)
    {
        _context = context;
    }
    public async Task<Attendance> AddAsync(Attendance attendance)
    {
        attendance.CreatedDate = DateTime.UtcNow;
        await _context.Attendances.AddAsync(attendance);
        return attendance;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var attendance = await _context.Attendances.FindAsync(id);
        if (attendance == null) return false;

        _context.Attendances.Remove(attendance);
        return true;
    }

    public async Task<Attendance?> GetByEmployeeIdAndDateAsync(Guid employeeId, DateTime date)
    {
        var checkInDate = DateOnly.FromDateTime(date);
        return await _context.Attendances
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && DateOnly.FromDateTime(a.CheckInDate) == checkInDate);
    }

    public async Task<Attendance> GetByIdAsync(Guid id)
    {
        return await _context.Attendances
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AttendanceId == id) ?? throw new KeyNotFoundException($"Attendance with ID {id} not found.");
    }

    public async Task<IEnumerable<Attendance>> GetMonthlyAttendanceAsync(Guid employeeId, int year, int month)
    {
     return await _context.Attendances
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId && a.CheckInDate.Year == year && a.CheckInDate.Month == month)
            .OrderBy(a => a.CheckInDate)
            .ToListAsync();
    }

    public async Task<int> GetMonthlyAttendanceCountAsync(int year, int month)
    {
        return await _context.Attendances
            .CountAsync(a => a.CheckInDate.Year == year && a.CheckInDate.Month == month);
    }

    public async Task<(PagedList<Attendance> Items, int TotalCount)> GetPaginatedAttendanceAsync(AttendanceQueryParams queryParams)
    {
        // Start query
        var query = _context.Attendances
            .AsNoTracking()
            .Include(a => a.Employee) 
            .AsQueryable();

        // Filters
        if (queryParams.EmployeeId.HasValue)
        {
            var employeeId = queryParams.EmployeeId.Value;
            query = query.Where(a => a.EmployeeId == employeeId);
        }

        if (queryParams.FromDate.HasValue)
        {
            var fromDate = DateOnly.FromDateTime(queryParams.FromDate.Value);
            query = query.Where(a => DateOnly.FromDateTime(a.CheckInDate) >= fromDate);
        }

        if (queryParams.ToDate.HasValue)
        {
            var toDate = DateOnly.FromDateTime(queryParams.ToDate.Value);
            query = query.Where(a => DateOnly.FromDateTime(a.CheckInDate) <= toDate);
        }

        // Sorting
        query = queryParams.SortOrder?.ToLower() switch
        {
            "employee" => query.OrderBy(a => a.Employee.FirstName).ThenBy(a => a.Employee.LastName),
            "-employee" => query.OrderByDescending(a => a.Employee.FirstName).ThenByDescending(a => a.Employee.LastName),
            "date" => query.OrderBy(a => a.CheckInDate).ThenBy(a => a.CheckInTime),
            "-date" => query.OrderByDescending(a => a.CheckInDate).ThenByDescending(a => a.CheckInTime),
            _ => query.OrderByDescending(a => a.CheckInDate) // default
        };

        // Execute query to allow filtering on computed property
        var filteredList = await query.ToListAsync();

        // Filter by computed Status (in memory)
        if (queryParams.Status.HasValue)
        {
            filteredList = filteredList.Where(a => a.Status == queryParams.Status.Value).ToList();
        }

        var totalCount = filteredList.Count;

        var pagedItems = filteredList
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToList();

        var pagedList = new PagedList<Attendance>(pagedItems, totalCount, queryParams.PageNumber, queryParams.PageSize);

        return (pagedList, totalCount);
    }
    public async Task<Attendance?> GetTodayAttendanceAsync(Guid employeeId)
    {
         return await _context.Attendances
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && DateOnly.FromDateTime(a.CheckInDate) == DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public async Task<int> GetTodayAttendanceCountAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _context.Attendances
            .CountAsync(a => DateOnly.FromDateTime(a.CheckInDate) == today);
    }

    public async Task<int> GetTotalAttendanceCountAsync()
    {
        return await _context.Attendances.CountAsync();
    }

    public async Task<IEnumerable<Attendance>> GetWeeklyAttendanceAsync(Guid employeeId, DateTime weekStartDate)
    {
        var weekStart = DateOnly.FromDateTime(weekStartDate);
        var weekEnd = weekStart.AddDays(6);
        return await _context.Attendances
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId && 
                        DateOnly.FromDateTime(a.CheckInDate) >= weekStart && 
                        DateOnly.FromDateTime(a.CheckInDate) <= weekEnd)
            .OrderBy(a => a.CheckInDate)
            .ToListAsync();
    }

    public Task<bool> HasCheckedInTodayAsync(Guid employeeId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return _context.Attendances
            .AsNoTracking()
            .AnyAsync(a => a.EmployeeId == employeeId && DateOnly.FromDateTime(a.CheckInDate) == today);
    }
}
