using FluentValidation;
using NodaTime;
using SzabiApp.Backend.Models.DTOs.Holidays;

namespace SzabiApp.Backend.Validators.Holidays;

public class CreateHolidayValidator : AbstractValidator<CreateHolidayDto>
{
    public CreateHolidayValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Holiday name is required.")
            .MaximumLength(200).WithMessage("Holiday name must not exceed 200 characters.");

        RuleFor(x => x.Date)
            .NotEqual(default(LocalDate)).WithMessage("Date is required.");
    }
}
