

namespace EmployeeManagementSys.DL
{
    public class Signature
    {
        public Guid SignatureId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
        public Guid EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }= null!;
    }
}
