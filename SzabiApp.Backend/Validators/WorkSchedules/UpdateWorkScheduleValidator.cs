using FluentValidation;
using SzabiApp.Backend.Models.DTOs.WorkSchedules;

namespace SzabiApp.Backend.Validators.WorkSchedules;

public class UpdateWorkScheduleValidator : AbstractValidator<UpdateWorkScheduleDto>
{
    public UpdateWorkScheduleValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.WorkDaysPerWeek)
            .InclusiveBetween(1, 7).WithMessage("Work days per week must be between 1 and 7.")
            .When(x => x.WorkDaysPerWeek is not null);

        RuleFor(x => x.DailyWorkHours)
            .InclusiveBetween(1, 24).WithMessage("Daily work hours must be between 1 and 24.")
            .When(x => x.DailyWorkHours is not null);
    }
}
