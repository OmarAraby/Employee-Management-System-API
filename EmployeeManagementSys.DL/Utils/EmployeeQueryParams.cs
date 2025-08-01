

namespace EmployeeManagementSys.DL
{
    public class EmployeeQueryParams
    {
        public string? SearchTerm { get; set; }
        public EmployeeStatus? Status { get; set; }
        public UserRole? Role { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedDate";
        public bool SortDescending { get; set; } = false;
    }
}
