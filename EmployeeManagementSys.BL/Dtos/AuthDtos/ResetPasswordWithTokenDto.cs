namespace EmployeeManagementSys.BL
{
    /// <summary>
    /// Completes a password reset: the email + the token from the emailed link,
    /// plus the new password. Distinct from ResetPasswordDto (the authenticated
    /// change-password flow that requires the current password).
    /// </summary>
    public class ResetPasswordWithTokenDto
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
