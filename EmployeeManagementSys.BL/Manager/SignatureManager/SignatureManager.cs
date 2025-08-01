using EmployeeManagementSys.DL;


namespace EmployeeManagementSys.BL
{
    public class SignatureManager : ISignatureManager
    {
        private readonly IUnitOfWork _unitOfWork;

        public SignatureManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<APIResult<SignatureDto>> UploadSignature(Guid empId, SignatureCreateDto dto)
        {
            
            string userRole = "Employee"; // 
            if (userRole != "Employee" && userRole != "Admin")
            {
                return new APIResult<SignatureDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "Unauthorized", Message = "Only employees or admins can upload signatures." } }
                };
            }

            // Validate employee existence
            var employee = await _unitOfWork.EmployeeRepository.GetByIDAsync(empId);
            if (employee == null)
            {
                return new APIResult<SignatureDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "NotFound", Message = "Employee not found." } }
                };
            }

            // Check if a signature already exists for this employee
            if (await _unitOfWork.SignatureRepository.ExistsByEmployeeIdAsync(empId))
            {
                return new APIResult<SignatureDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "DuplicateSignature", Message = "A signature already exists for this employee." } }
                };
            }

            // Create and save the new signature
            var signature = new Signature
            {
                FileName = dto.FileName,
                FilePath = dto.FileUrl,
                UploadedAt = DateTime.UtcNow,
                EmployeeId = empId
            };

            var addedSignature = await _unitOfWork.SignatureRepository.AddAsync(signature);
            await _unitOfWork.SaveChangesAsync();

            return new APIResult<SignatureDto>
            {
                Success = true,
                Data = new SignatureDto
                {
                    SignatureId = addedSignature.SignatureId,
                    FileName = addedSignature.FileName,
                    FilePath = addedSignature.FilePath,
                    UploadedAt = addedSignature.UploadedAt,
                    EmployeeId = addedSignature.EmployeeId
                }
            };
        }

        public async Task<APIResult<SignatureDto>> GetSignaturesForEmployee(Guid empId)
        {
          
            string userRole = "Employee"; 
            if (userRole != "Employee" && userRole != "Admin")
            {
                return new APIResult<SignatureDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "Unauthorized", Message = "Only employees or admins can view signatures." } }
                };
            }

            // Validate employee existence
            var employee = await _unitOfWork.EmployeeRepository.GetByIDAsync(empId);
            if (employee == null)
            {
                return new APIResult<SignatureDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "NotFound", Message = "Employee not found." } }
                };
            }

            // Retrieve the signature
            var signature = await _unitOfWork.SignatureRepository.GetByEmployeeIdAsync(empId);
            if (signature == null)
            {
                return new APIResult<SignatureDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "NotFound", Message = "No signature found for this employee." } }
                };
            }

            return new APIResult<SignatureDto>
            {
                Success = true,
                Data = new SignatureDto
                {
                    SignatureId = signature.SignatureId,
                    FileName = signature.FileName,
                    FilePath = signature.FilePath,
                    UploadedAt = signature.UploadedAt,
                    EmployeeId = signature.EmployeeId
                }
            };
        }

       
    }
}