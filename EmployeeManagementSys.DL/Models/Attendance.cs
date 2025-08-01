

namespace EmployeeManagementSys.DL
{
    public class Attendance
    {
        public Guid AttendanceId { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime CheckInDate { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public double? WorkingHours { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Employee Employee { get; set; }

        // Computed properties (these don't need configuration as they're not mapped)
        public string CheckInDateString => CheckInDate.ToString("yyyy-MM-dd");
        public string CheckInTimeString => CheckInTime.ToString(@"hh\:mm");
        public bool IsOnTime => CheckInTime <= new TimeSpan(9, 0, 0);
        public bool IsLate => CheckInTime > new TimeSpan(9, 0, 0);
        public AttendanceStatus Status => IsOnTime ? AttendanceStatus.OnTime : AttendanceStatus.Late;
    }
}
