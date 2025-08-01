using Microsoft.EntityFrameworkCore;


namespace EmployeeManagementSys.DL;
public class EmployeeRepository : IEmployeeRepository
{
    private readonly EmployeeManagementSysDbContext _context;
    public EmployeeRepository(EmployeeManagementSysDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Employee> AddAsync(Employee employee)
    {
        if (employee == null)
        {
            throw new ArgumentNullException(nameof(employee));
        }

        employee.CreatedDate = DateTime.UtcNow;
        await _context.Employees.AddAsync(employee);
        return employee; 
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if(employee == null) return false;

         _context.Employees.Remove(employee);
        return true;

    }

    public Task<int> GetActiveEmployeesCountAsync()
    {
        return _context.Employees.CountAsync(e => e.Status == EmployeeStatus.Active);
    }

    public async Task<Employee> GetByIDAsync(Guid id)
    {
      return await _context.Employees.FindAsync(id) ?? throw new KeyNotFoundException($"Employee with ID {id} not found.");  
    }

    public async Task<(PagedList<Employee> Items, int TotalCount)> GetPaginatedEmployeesAsync(EmployeeQueryParams queryParams)
    {
        var query = _context.Employees.AsNoTracking().AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            query = query.Where(e =>
                (e.FirstName + " " + e.LastName).Contains(queryParams.SearchTerm) ||
                e.Email.Contains(queryParams.SearchTerm) ||
                e.NationalId.Contains(queryParams.SearchTerm));
        }

        if (queryParams.Status.HasValue)
        {
            query = query.Where(e => e.Status == queryParams.Status.Value);
        }

        if (queryParams.MinAge.HasValue)
        {
            query = query.Where(e => e.Age >= queryParams.MinAge.Value);
        }

        if (queryParams.MaxAge.HasValue)
        {
            query = query.Where(e => e.Age <= queryParams.MaxAge.Value);
        }

        // Sorting - Using SortBy + SortDescending
        query = queryParams.SortBy?.ToLower() switch
        {
            "name" or "firstname" or "lastname" => queryParams.SortDescending
                ? query.OrderByDescending(e => e.FirstName).ThenByDescending(e => e.LastName)
                : query.OrderBy(e => e.FirstName).ThenBy(e => e.LastName),

            "email" => queryParams.SortDescending
                ? query.OrderByDescending(e => e.Email)
                : query.OrderBy(e => e.Email),

            "age" => queryParams.SortDescending
                ? query.OrderByDescending(e => e.Age)
                : query.OrderBy(e => e.Age),

            "status" => queryParams.SortDescending
                ? query.OrderByDescending(e => e.Status)
                : query.OrderBy(e => e.Status),

            "createddate" => queryParams.SortDescending
                ? query.OrderByDescending(e => e.CreatedDate)
                : query.OrderBy(e => e.CreatedDate),

            _ => queryParams.SortDescending
                ? query.OrderByDescending(e => e.CreatedDate)
                : query.OrderBy(e => e.CreatedDate) // safe default
        };

        var totalCount = await query.CountAsync();

        var employees = await query
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var pagedList = new PagedList<Employee>(employees, totalCount, queryParams.PageNumber, queryParams.PageSize);

        return (pagedList, totalCount);
    }
    public async Task<int> GetTotalEmployeesAsync()
    {
        return await _context.Employees.CountAsync();
    }

    public async Task<Employee?> GetWithAttendanceAsync(Guid employeeId)
    {
        return await _context.Employees
            .Include(e => e.AttendanceRecords)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employeeId);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null)
    {
        var query = _context.Employees.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }
        return !await query.AnyAsync(e => e.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> IsNationalIdUniqueAsync(string nationalId, Guid? excludeId = null)
    {
        var query = _context.Employees.AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }

        return !await query.AnyAsync(e => e.NationalId == nationalId);
    }

    public async Task<Employee> UpdateAsync(Employee employee)
    {
        var existingEmployee = await _context.Employees.FindAsync(employee.Id);
        if (existingEmployee == null) return null;

        employee.UpdatedDate = DateTime.UtcNow;
        _context.Entry(existingEmployee).CurrentValues.SetValues(employee);
        return existingEmployee; 


    }
}

