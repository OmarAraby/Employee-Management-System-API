

namespace EmployeeManagementSys.BL
{
    public interface ISignatureManager
    {
        // File Management Methods
        Task<APIResult<SignatureDto>> UploadSignature(Guid empId, SignatureCreateDto dto, string userRole, Guid callerId);
        Task<APIResult<SignatureDto>> GetSignaturesForEmployee(Guid empId, string userRole, Guid callerId);
        //Task<APIResult> DeleteSignature(Guid empId, Guid signatureId);
    }
}
