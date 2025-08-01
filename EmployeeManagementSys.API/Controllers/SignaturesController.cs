using EmployeeManagementSys.BL;
using EmployeeManagementSys.API.HandleFiles;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

[Route("api/signatures")]
[ApiController]
[Authorize]
public class SignatureController : ControllerBase
{
    private readonly ISignatureManager _signatureManager;
    private readonly IFileService _fileService;

    public SignatureController(ISignatureManager signatureManager, IFileService fileService)
    {
        _signatureManager = signatureManager;
        _fileService = fileService;
    }

    [HttpPost("upload/{empId}")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<Results<Ok<APIResult<SignatureDto>>, BadRequest<APIResult<SignatureDto>>>> UploadSignature(
        Guid empId,
        [FromForm] FileUploadRequest fileRequest)
    {
        try
        {
            if (fileRequest == null || fileRequest.File == null || fileRequest.File.Length == 0)
            {
                return TypedResults.BadRequest(new APIResult<SignatureDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "FILE_NULL", Message = "No file was provided or file is empty" } }
                });
            }

            var uploadResult = await _fileService.UploadFileAsync(fileRequest.File);
            var dto = new SignatureCreateDto
            {
                FileName = fileRequest.File.FileName,
                FileUrl = uploadResult.FileUrl,
                EmployeeId = empId 
            };

            var result = await _signatureManager.UploadSignature(empId, dto);
            return result.Success
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(new APIResult<SignatureDto>
            {
                Success = false,
                Errors = new[] { new APIError { Code = "FILE_UPLOAD_ERROR", Message = ex.Message } }
            });
        }
    }

    [HttpGet("{empId}")]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<Results<Ok<APIResult<SignatureDto>>, NotFound<APIResult<SignatureDto>>>> GetSignaturesForEmployee(Guid empId)
    {
        var result = await _signatureManager.GetSignaturesForEmployee(empId);
        return result.Success
            ? TypedResults.Ok(result)
            : TypedResults.NotFound(result);
    }

   
   
}