using NodaTime;

namespace SzabiApp.Backend.Models.DTOs.Holidays;

public class HolidayDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public LocalDate Date { get; set; }
    public bool IsRecurringYearly { get; set; }
}
