
namespace EmployeeManagementSys.DL
{
    public interface IEmployeeRepository
    {
        Task<Employee> GetByIDAsync(Guid id);
        Task<Employee> AddAsync(Employee employee);
        Task<Employee> UpdateAsync(Employee employee);
        Task<bool> DeleteAsync(Guid id);


        Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null);
        Task<bool> IsNationalIdUniqueAsync(string nationalId, Guid? excludeId = null);


        Task<Employee?> GetWithAttendanceAsync(Guid employeeId);

        Task<(PagedList<Employee> Items, int TotalCount)> GetPaginatedEmployeesAsync(EmployeeQueryParams queryParams);

        // Statistics
        Task<int> GetTotalEmployeesAsync();
        Task<int> GetActiveEmployeesCountAsync();

        // authentication-related methods
        Task<Employee?> GetByEmailAsync(string email);
        Task<Employee?> GetByUsernameAsync(string username);
        Task<bool> UpdatePasswordResetStatusAsync(Guid employeeId, bool requiresReset);
        Task<IEnumerable<Employee>> GetEmployeesRequiringPasswordResetAsync();



    }
}
