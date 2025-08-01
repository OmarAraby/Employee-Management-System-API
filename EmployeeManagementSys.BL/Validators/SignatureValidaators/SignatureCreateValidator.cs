
using FluentValidation;

namespace EmployeeManagementSys.BL
{
    public class SignatureCreateValidator : AbstractValidator<SignatureCreateDto>
    {
        public SignatureCreateValidator()
        {
            RuleFor(dto => dto.EmployeeId)
                .NotEmpty().WithMessage("EmployeeId must be a valid GUID")
                .NotEqual(Guid.Empty).WithMessage("EmployeeId must be a valid GUID");

            RuleFor(dto => dto.FileUrl)
                .NotEmpty().WithMessage("File URL is required")
                .Must(url => Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute)).WithMessage("File URL must be a valid URL");

            RuleFor(dto => dto.FileName)
                .NotEmpty().WithMessage("File name is required");
        }
    }
}
