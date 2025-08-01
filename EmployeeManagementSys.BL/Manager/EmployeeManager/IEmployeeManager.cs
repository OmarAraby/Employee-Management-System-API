

using EmployeeManagementSys.DL;

namespace EmployeeManagementSys.BL
{
    public interface IEmployeeManager
    {
        Task<APIResult<EmployeeDto>> AddEmployeeAsync(CreateEmployeeDto createDto, string userRole);
        Task<APIResult<EmployeeDto>> UpdateEmployeeAsync(EmployeeDto updateDto, string userRole);
        Task<APIResult<bool>> DeleteEmployeeAsync(Guid id, string userRole);
        Task<APIResult<(PagedList<EmployeeListDto> Items, int TotalCount)>> GetPaginatedEmployeesAsync(EmployeeQueryParams queryParams);
        Task<APIResult<EmployeeStatsDto>> GetEmployeeStatisticsAsync();
        Task<APIResult<EmployeeDto>> GetEmployeeProfileAsync(Guid employeeId, string userRole);

    }
}
