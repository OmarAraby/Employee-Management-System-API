using EmployeeManagementSys.DL;


namespace EmployeeManagementSys.BL
{
    public class AttendanceListDto
    {
        public Guid AttendanceId { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeFullName { get; set; }
        public DateTime CheckInDate { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public bool IsOnTime { get; set; }
        public AttendanceStatus Status { get; set; }
        public string StatusDisplayName { get; set; }
    }
}
