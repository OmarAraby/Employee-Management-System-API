using EmployeeManagementSys.DL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EmployeeManagementSys.DAL
{
    public class SignatureRepository : ISignatureRepository
    {
        private readonly EmployeeManagementSysDbContext _context;

        public SignatureRepository(EmployeeManagementSysDbContext context)
        {
            _context = context;
        }

        public async Task<Signature?> GetByEmployeeIdAsync(Guid employeeId)
        {
            return await _context.Signatures
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId);
        }

        public async Task<Signature?> GetByIdAsync(Guid signatureId)
        {
            return await _context.Signatures
                .AsNoTracking()
                .Include(s => s.Employee)
                .FirstOrDefaultAsync(s => s.SignatureId == signatureId);
        }

        public async Task<Signature> AddAsync(Signature signature)
        {
            signature.UploadedAt = DateTime.UtcNow;
            await _context.Signatures.AddAsync(signature);
            return signature;
        }

        public async Task<bool> UpdateAsync(Signature signature)
        {
            var existing = await _context.Signatures.FindAsync(signature.SignatureId);
            if (existing == null) return false;

            existing.FileName = signature.FileName;
            existing.FilePath = signature.FilePath;
            // UploadedAt not updated on edit

            _context.Signatures.Update(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid signatureId)
        {
            var signature = await _context.Signatures.FindAsync(signatureId);
            if (signature == null) return false;

            _context.Signatures.Remove(signature);
            return true;
        }

        public async Task<bool> ExistsByEmployeeIdAsync(Guid employeeId)
        {
            return await _context.Signatures
                .AnyAsync(s => s.EmployeeId == employeeId);
        }
    }
}