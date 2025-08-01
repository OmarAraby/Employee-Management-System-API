
namespace EmployeeManagementSys.BL
{
    public class SignatureCreateDto
    {
        public string FileUrl { get; set; } // URL of the uploaded file
        public string FileName { get; set; } // Name of the file
        public Guid EmployeeId { get; set; }
    }
}
