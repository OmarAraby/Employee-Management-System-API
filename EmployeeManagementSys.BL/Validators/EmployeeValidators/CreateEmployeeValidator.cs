using FluentValidation;

namespace EmployeeManagementSys.BL
{
    public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeDto>
    {
        public CreateEmployeeValidator()
        {
            RuleFor(e => e.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(e => e.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(e => e.Age)
                .InclusiveBetween(18, 65).WithMessage("Age must be between 18 and 65.");

            RuleFor(e => e.NationalId)
                .NotEmpty().WithMessage("National ID is required.")
                .Length(14).WithMessage("National ID must be exactly 14 digits.");
        }
    }
}