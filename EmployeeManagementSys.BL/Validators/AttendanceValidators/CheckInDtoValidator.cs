

using FluentValidation;

namespace EmployeeManagementSys.BL
{
    public class CheckInDtoValidator : AbstractValidator<CheckInDto>
    {
        public CheckInDtoValidator()
        {
            RuleFor(c => c.EmployeeId)
                .NotEmpty().WithMessage("Employee ID is required.");
        }
    }
}
