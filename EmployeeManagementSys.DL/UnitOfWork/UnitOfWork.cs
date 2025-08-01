

namespace EmployeeManagementSys.DL;

public class UnitOfWork : IUnitOfWork ,IDisposable
{
    private readonly EmployeeManagementSysDbContext _context;
    public IEmployeeRepository EmployeeRepository { get; }
    public IAttendanceRepository AttendanceRepository { get; }
    public ISignatureRepository SignatureRepository { get; }


    public UnitOfWork(EmployeeManagementSysDbContext context, IEmployeeRepository employeeRepository , IAttendanceRepository attendanceRepository , ISignatureRepository signatureRepository) 
    {
        _context = context ;
        EmployeeRepository = employeeRepository ;
        AttendanceRepository = attendanceRepository ;
        SignatureRepository = signatureRepository;
    }
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

