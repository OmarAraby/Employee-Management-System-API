

namespace EmployeeManagementSys.BL
{
    public class RefreshTokenResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string NewToken { get; set; } = string.Empty;
        public DateTime TokenExpiry { get; set; }
    }
}
