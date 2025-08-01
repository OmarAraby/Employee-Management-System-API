

using EmployeeManagementSys.DL;

namespace EmployeeManagementSys.BL
{
    public  class EmployeeDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string NationalId { get; set; }
        public int Age { get; set; }
        public string? Signature { get; set; }
        public EmployeeStatus Status { get; set; }
        public string StatusDisplayName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; }

        public string Email { get; set; } = string.Empty;   
    }
}
