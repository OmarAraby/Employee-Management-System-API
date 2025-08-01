

namespace EmployeeManagementSys.BL
{
    public class SignatureDto
    {
        public Guid SignatureId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
        public Guid EmployeeId { get; set; }
    }
}
