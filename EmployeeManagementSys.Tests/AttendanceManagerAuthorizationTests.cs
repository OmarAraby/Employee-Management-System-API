using EmployeeManagementSys.BL;
using EmployeeManagementSys.DL;
using Moq;
using Xunit;

namespace EmployeeManagementSys.Tests;

/// <summary>
/// Guards the ownership / role authorization branches on the attendance
/// endpoints (the IDOR fixes from #9). All denial paths must return before any
/// repository access, so a strict mock that is never invoked proves it.
/// </summary>
public class AttendanceManagerAuthorizationTests
{
    private static AttendanceManager BuildManager(out Mock<IUnitOfWork> uow)
    {
        uow = new Mock<IUnitOfWork>(MockBehavior.Strict);
        return new AttendanceManager(uow.Object, new CheckInDtoValidator());
    }

    [Fact]
    public async Task CheckIn_NonEmployeeRole_IsDenied()
    {
        var mgr = BuildManager(out var uow);

        var result = await mgr.CheckInAsync(new CheckInDto { EmployeeId = Guid.NewGuid() }, "Admin", Guid.NewGuid());

        Assert.False(result.Success);
        uow.VerifyNoOtherCalls(); // denied before touching the repo
    }

    [Fact]
    public async Task Weekly_EmployeeViewingAnother_IsDenied()
    {
        var mgr = BuildManager(out var uow);
        var target = Guid.NewGuid();
        var caller = Guid.NewGuid(); // different person

        var result = await mgr.GetWeeklyAttendanceAsync(target, "Employee", caller);

        Assert.False(result.Success);
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Monthly_EmployeeViewingAnother_IsDenied()
    {
        var mgr = BuildManager(out var uow);
        var target = Guid.NewGuid();
        var caller = Guid.NewGuid();

        var result = await mgr.GetMonthlyAttendanceAsync(target, 2026, 7, "Employee", caller);

        Assert.False(result.Success);
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Monthly_UnknownRole_IsDenied()
    {
        var mgr = BuildManager(out var uow);

        var result = await mgr.GetMonthlyAttendanceAsync(Guid.NewGuid(), 2026, 7, "", Guid.NewGuid());

        Assert.False(result.Success);
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Report_EmployeeViewingAnother_IsDenied()
    {
        var mgr = BuildManager(out var uow);
        var target = Guid.NewGuid();
        var caller = Guid.NewGuid();

        var result = await mgr.GetMonthlyAttendanceReportCsvAsync(target, 2026, 7, "Employee", caller);

        Assert.False(result.Success);
        uow.VerifyNoOtherCalls();
    }
}
