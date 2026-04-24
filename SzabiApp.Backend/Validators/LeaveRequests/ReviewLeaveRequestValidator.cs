using FluentValidation;
using SzabiApp.Backend.Models.DTOs.LeaveRequests;
using SzabiApp.Backend.Models.Enums;

namespace SzabiApp.Backend.Validators.LeaveRequests;

public class ReviewLeaveRequestValidator : AbstractValidator<ReviewLeaveRequestDto>
{
    public ReviewLeaveRequestValidator()
    {
        RuleFor(x => x.Decision)
            .Must(d => d == LeaveStatus.Approved || d == LeaveStatus.Denied)
            .WithMessage("Decision must be either Approved or Denied.");

        RuleFor(x => x.ReviewNote)
            .MaximumLength(1000).WithMessage("Review note must not exceed 1000 characters.")
            .When(x => x.ReviewNote is not null);
    }
}
