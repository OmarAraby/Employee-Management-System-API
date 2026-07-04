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

    [Fact]
    public async Task CheckOut_NonEmployeeRole_IsDenied()
    {
        var mgr = BuildManager(out var uow);

        var result = await mgr.CheckOutAsync("Admin", Guid.NewGuid());

        Assert.False(result.Success);
        uow.VerifyNoOtherCalls(); // role denial before any repo access
    }

    [Fact]
    public async Task CheckOut_NotCheckedInToday_IsDenied()
    {
        // Loose mock: this path DOES touch the repo (to look for today's record).
        var uow = new Mock<IUnitOfWork>();
        var attendanceRepo = new Mock<IAttendanceRepository>();
        attendanceRepo.Setup(r => r.GetTodayAttendanceAsync(It.IsAny<Guid>()))
                      .ReturnsAsync((Attendance?)null);
        uow.SetupGet(u => u.AttendanceRepository).Returns(attendanceRepo.Object);
        var mgr = new AttendanceManager(uow.Object, new CheckInDtoValidator());

        var result = await mgr.CheckOutAsync("Employee", Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Contains(result.Errors!, e => e.Code == "NotCheckedIn");
    }

    [Fact]
    public async Task CheckOut_AlreadyCheckedOut_IsDenied()
    {
        var caller = Guid.NewGuid();
        var uow = new Mock<IUnitOfWork>();
        var attendanceRepo = new Mock<IAttendanceRepository>();
        attendanceRepo.Setup(r => r.GetTodayAttendanceAsync(caller))
                      .ReturnsAsync(new Attendance
                      {
                          AttendanceId = Guid.NewGuid(),
                          EmployeeId = caller,
                          CheckInDate = DateTime.UtcNow.Date,
                          CheckInTime = new TimeSpan(8, 0, 0),
                          CheckOutTime = new TimeSpan(17, 0, 0) // already checked out
                      });
        uow.SetupGet(u => u.AttendanceRepository).Returns(attendanceRepo.Object);
        var mgr = new AttendanceManager(uow.Object, new CheckInDtoValidator());

        var result = await mgr.CheckOutAsync("Employee", caller);

        Assert.False(result.Success);
        Assert.Contains(result.Errors!, e => e.Code == "AlreadyCheckedOut");
        attendanceRepo.Verify(r => r.UpdateAsync(It.IsAny<Attendance>()), Times.Never);
    }

    [Fact]
    public async Task CheckOut_HappyPath_SetsCheckOutAndWorkingHours()
    {
        var caller = Guid.NewGuid();
        var record = new Attendance
        {
            AttendanceId = Guid.NewGuid(),
            EmployeeId = caller,
            CheckInDate = DateTime.UtcNow.Date,
            // Midnight check-in so WorkingHours = (checkout-time-of-day − 0) is
            // non-negative regardless of the real wall-clock the manager reads
            // (the manager computes checkout time itself; the test can't inject it).
            CheckInTime = TimeSpan.Zero,
            CheckOutTime = null
        };
        var uow = new Mock<IUnitOfWork>();
        var attendanceRepo = new Mock<IAttendanceRepository>();
        var employeeRepo = new Mock<IEmployeeRepository>();
        attendanceRepo.Setup(r => r.GetTodayAttendanceAsync(caller)).ReturnsAsync(record);
        attendanceRepo.Setup(r => r.UpdateAsync(It.IsAny<Attendance>())).ReturnsAsync(record);
        employeeRepo.Setup(r => r.GetByIDAsync(caller)).ReturnsAsync((Employee?)null);
        uow.SetupGet(u => u.AttendanceRepository).Returns(attendanceRepo.Object);
        uow.SetupGet(u => u.EmployeeRepository).Returns(employeeRepo.Object);
        uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        var mgr = new AttendanceManager(uow.Object, new CheckInDtoValidator());

        var result = await mgr.CheckOutAsync("Employee", caller);

        Assert.True(result.Success);
        // The manager set CheckOutTime + WorkingHours on the record before persisting.
        Assert.True(record.CheckOutTime.HasValue);
        Assert.NotNull(record.WorkingHours);
        Assert.True(record.WorkingHours >= 0); // wall-clock-independent (check-in at midnight)
        attendanceRepo.Verify(r => r.UpdateAsync(record), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
