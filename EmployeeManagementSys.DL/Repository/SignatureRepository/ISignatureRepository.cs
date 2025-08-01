

namespace EmployeeManagementSys.DL
{
    public interface ISignatureRepository
    {
        Task<Signature?> GetByEmployeeIdAsync(Guid employeeId);
        Task<Signature?> GetByIdAsync(Guid signatureId);
        Task<Signature> AddAsync(Signature signature);
        Task<bool> UpdateAsync(Signature signature);
        Task<bool> DeleteAsync(Guid signatureId);
        Task<bool> ExistsByEmployeeIdAsync(Guid employeeId);


    }
}
