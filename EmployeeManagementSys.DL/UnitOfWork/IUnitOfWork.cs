

namespace EmployeeManagementSys.DL;

public interface IUnitOfWork
{
    IEmployeeRepository EmployeeRepository { get; }
    IAttendanceRepository AttendanceRepository { get; }
    
    Task<int> SaveChangesAsync();
  
}
