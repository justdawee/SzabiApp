using NodaTime;

namespace SzabiApp.Backend.Models.Entities;

public class Holiday
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public LocalDate Date { get; set; }
    public bool IsRecurringYearly { get; set; }
}
