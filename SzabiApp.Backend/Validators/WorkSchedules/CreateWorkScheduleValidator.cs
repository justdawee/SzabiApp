using FluentValidation;
using SzabiApp.Backend.Models.DTOs.WorkSchedules;

namespace SzabiApp.Backend.Validators.WorkSchedules;

public class CreateWorkScheduleValidator : AbstractValidator<CreateWorkScheduleDto>
{
    public CreateWorkScheduleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.WorkDaysPerWeek)
            .InclusiveBetween(1, 7).WithMessage("Work days per week must be between 1 and 7.");

        RuleFor(x => x.DailyWorkHours)
            .InclusiveBetween(1, 24).WithMessage("Daily work hours must be between 1 and 24.");
    }
}
