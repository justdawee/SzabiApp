using NodaTime;

namespace SzabiApp.Backend.Models.DTOs.Holidays;

public class CreateHolidayDto
{
    public string Name { get; set; } = string.Empty;
    public LocalDate Date { get; set; }
    public bool IsRecurringYearly { get; set; }
}
