namespace SzabiApp.Backend.Models.Entities;

public class WorkSchedule
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int WorkDaysPerWeek { get; set; }
    public int DailyWorkHours { get; set; }

    public ICollection<User> Users { get; set; } = [];
}
