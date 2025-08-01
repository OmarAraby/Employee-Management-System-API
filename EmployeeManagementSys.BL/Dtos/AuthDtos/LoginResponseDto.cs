

namespace EmployeeManagementSys.BL
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool RequiresPasswordReset { get; set; }
        public string Token { get; set; } = string.Empty; // JWT Token
        public DateTime TokenExpiry { get; set; } 
    }
}
