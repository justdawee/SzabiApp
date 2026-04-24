using FluentValidation;
using SzabiApp.Backend.Models.DTOs.LeaveAllowances;

namespace SzabiApp.Backend.Validators.LeaveAllowances;

public class CreateLeaveAllowanceValidator : AbstractValidator<CreateLeaveAllowanceDto>
{
    public CreateLeaveAllowanceValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100).WithMessage("Year must be between 2000 and 2100.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category must be a valid leave category.");

        RuleFor(x => x.TotalDays)
            .GreaterThan(0).WithMessage("Total days must be greater than 0.")
            .LessThanOrEqualTo(365).WithMessage("Total days cannot exceed 365.");
    }
}
