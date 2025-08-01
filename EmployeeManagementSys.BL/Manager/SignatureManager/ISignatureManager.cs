

namespace EmployeeManagementSys.BL
{
    public interface ISignatureManager
    {
        // File Management Methods
        Task<APIResult<SignatureDto>> UploadSignature(Guid empId, SignatureCreateDto dto);
        Task<APIResult<SignatureDto>> GetSignaturesForEmployee(Guid empId);
        //Task<APIResult> DeleteSignature(Guid empId, Guid signatureId);
    }
}
