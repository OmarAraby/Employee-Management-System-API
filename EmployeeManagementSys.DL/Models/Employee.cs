using Microsoft.AspNetCore.Identity;

namespace EmployeeManagementSys.DL
{
    public class Employee : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalId { get; set; }
        public int Age { get; set; }
        public string? Signature { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

        // Navigation property for attendance records
        public virtual ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();

        // Computed properties 
        public string FullName => $"{FirstName} {LastName}";
        public bool IsActive => Status == EmployeeStatus.Active;
        public string StatusDisplayName => Status.ToString();
    }
}
