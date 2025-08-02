
using EmployeeManagementSys.DL;

namespace EmployeeManagementSys
{
    public class EmployeeListDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string NationalId { get; set; }
        public int Age { get; set; }
        public EmployeeStatus Status { get; set; }
        public string StatusDisplayName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
