using EmployeeManagementSys.BL;
using EmployeeManagementSys.API.Extensions;
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

            var userRole = User.GetRole();
            if (!User.TryGetUserId(out var callerId))
            {
                return TypedResults.BadRequest(new APIResult<SignatureDto>
                {
                    Success = false,
                    Errors = new[] { new APIError { Code = "Unauthorized", Message = "Caller identity could not be determined." } }
                });
            }

            var uploadResult = await _fileService.UploadFileAsync(fileRequest.File);
            var dto = new SignatureCreateDto
            {
                FileName = fileRequest.File.FileName,
                FileUrl = uploadResult.FileUrl,
                EmployeeId = empId
            };

            var result = await _signatureManager.UploadSignature(empId, dto, userRole, callerId);
            return result.Success
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        }
        catch (Exception)
        {
            // Do not leak the raw exception message to the client (CWE-209).
            return TypedResults.BadRequest(new APIResult<SignatureDto>
            {
                Success = false,
                Errors = new[] { new APIError { Code = "FILE_UPLOAD_ERROR", Message = "The signature could not be uploaded. Please try again." } }
            });
        }
    }

    [HttpGet("{empId}")]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<Results<Ok<APIResult<SignatureDto>>, NotFound<APIResult<SignatureDto>>, ForbidHttpResult>> GetSignaturesForEmployee(Guid empId)
    {
        var userRole = User.GetRole();
        if (!User.TryGetUserId(out var callerId)) return TypedResults.Forbid();
        var result = await _signatureManager.GetSignaturesForEmployee(empId, userRole, callerId);
        return result.Success
            ? TypedResults.Ok(result)
            : TypedResults.NotFound(result);
    }

   
   
}