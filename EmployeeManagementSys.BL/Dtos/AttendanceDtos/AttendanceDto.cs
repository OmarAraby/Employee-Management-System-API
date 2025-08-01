using EmployeeManagementSys.DL;

namespace EmployeeManagementSys.BL
{
    public class AttendanceDto
    {
        public Guid AttendanceId { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeFullName { get; set; }
        public string EmployeeEmail { get; set; }
        public DateTime CheckInDate { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public double? WorkingHours { get; set; }
        public DateTime CreatedDate { get; set; }

        // Computed properties
        public string CheckInDateString { get; set; }
        public string CheckInTimeString { get; set; }
        public bool IsOnTime { get; set; }
        public bool IsLate { get; set; }
        public AttendanceStatus Status { get; set; }
        public string StatusDisplayName { get; set; }
    }
}
