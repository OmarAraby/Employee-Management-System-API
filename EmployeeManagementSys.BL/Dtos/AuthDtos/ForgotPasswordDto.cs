namespace EmployeeManagementSys.BL
{
    /// <summary>Request to start a password reset — just the account email.</summary>
    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }
}
