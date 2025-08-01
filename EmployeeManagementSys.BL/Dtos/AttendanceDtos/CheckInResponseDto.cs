

namespace EmployeeManagementSys.BL
{
    public class CheckInResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public AttendanceDto? Attendance { get; set; }
        public TimeSpan? CheckInTime { get; set; }
        public DateTime CheckInDate { get; set; }
    }
}
