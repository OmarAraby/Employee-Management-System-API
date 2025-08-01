using Microsoft.AspNetCore.Identity;

namespace EmployeeManagementSys.DL
{
    public class Employee : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalId { get; set; }
        public int Age { get; set; }
        public Guid? SignatureId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

        // Add this property to track if employee needs to reset password
        public bool RequiresPasswordReset { get; set; } = true;
        public DateTime? LastPasswordResetDate { get; set; }

        // Navigation property for attendance records
        public virtual ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();

        public virtual Signature? Signature { get; set; }


        // Computed properties 
        public string FullName => $"{FirstName} {LastName}";
        public bool IsActive => Status == EmployeeStatus.Active;
        public string StatusDisplayName => Status.ToString();
    }
 
}
