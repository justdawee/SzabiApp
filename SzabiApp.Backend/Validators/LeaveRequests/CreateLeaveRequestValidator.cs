using FluentValidation;
using NodaTime;
using SzabiApp.Backend.Models.DTOs.LeaveRequests;

namespace SzabiApp.Backend.Validators.LeaveRequests;

public class CreateLeaveRequestValidator : AbstractValidator<CreateLeaveRequestDto>
{
    public CreateLeaveRequestValidator(IClock clock)
    {
        var today = clock.GetCurrentInstant().InUtc().Date;

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category must be a valid leave category.");

        RuleFor(x => x.StartDate)
            .NotEqual(default(LocalDate)).WithMessage("Start date is required.")
            .GreaterThanOrEqualTo(today).WithMessage("Start date cannot be in the past.");

        RuleFor(x => x.EndDate)
            .NotEqual(default(LocalDate)).WithMessage("End date is required.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be on or after the start date.");

        RuleFor(x => x.RequestNote)
            .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.")
            .When(x => x.RequestNote is not null);
    }
}
