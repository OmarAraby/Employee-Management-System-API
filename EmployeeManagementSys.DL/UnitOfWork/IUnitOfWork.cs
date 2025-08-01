

namespace EmployeeManagementSys.DL;

public interface IUnitOfWork
{
    IEmployeeRepository EmployeeRepository { get; }
    IAttendanceRepository AttendanceRepository { get; }
    ISignatureRepository SignatureRepository { get; }

    Task<int> SaveChangesAsync();
  
}
