

namespace EmployeeManagementSys.DL
{
    public class AttendanceQueryParams
    {
        public string? SearchTerm { get; set; }
        public Guid? EmployeeId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public bool IncludeEmployee { get; set; }   = true;
        public string? SortOrder { get; set; }
        public AttendanceStatus? Status { get; set; }

    }
}
