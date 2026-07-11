namespace EmployeeManagementSys.BL
{
    /// <summary>
    /// Sends transactional email. Defined consumer-side (BL) so managers depend
    /// on the abstraction; the SMTP implementation lives in the API layer.
    /// </summary>
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
    }
}
