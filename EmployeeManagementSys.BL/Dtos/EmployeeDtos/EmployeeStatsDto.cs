

namespace EmployeeManagementSys.BL
{
    public class EmployeeStatsDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int InactiveEmployees { get; set; }
        public int SuspendedEmployees { get; set; }
        public int NewEmployeesThisMonth { get; set; }
    }
}
