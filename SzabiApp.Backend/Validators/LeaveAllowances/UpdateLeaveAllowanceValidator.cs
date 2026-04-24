using FluentValidation;
using SzabiApp.Backend.Models.DTOs.LeaveAllowances;

namespace SzabiApp.Backend.Validators.LeaveAllowances;

public class UpdateLeaveAllowanceValidator : AbstractValidator<UpdateLeaveAllowanceDto>
{
    public UpdateLeaveAllowanceValidator()
    {
        RuleFor(x => x.TotalDays)
            .GreaterThan(0).WithMessage("Total days must be greater than 0.")
            .LessThanOrEqualTo(365).WithMessage("Total days cannot exceed 365.");
    }
}
