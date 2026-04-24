using FluentValidation;
using SzabiApp.Backend.Models.DTOs.Users;

namespace SzabiApp.Backend.Validators.Users;

public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.")
            .When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.")
            .When(x => x.LastName is not null);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.")
            .When(x => x.Email is not null);

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role must be a valid value (Employee, Manager, Admin).")
            .When(x => x.Role is not null);
    }
}
