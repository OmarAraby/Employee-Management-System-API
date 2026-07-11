namespace EmployeeManagementSys.API.Email
{
    /// <summary>Bound from the "Email" configuration section (env: Email__*).</summary>
    public class EmailSettings
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 25;
        public string From { get; set; } = "noreply@ems.local";
        public string? User { get; set; }
        public string? Password { get; set; }
        public bool UseSsl { get; set; }
    }
}
