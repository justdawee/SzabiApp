namespace SzabiApp.Backend.Models.DTOs.WorkSchedules;

public class WorkScheduleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int WorkDaysPerWeek { get; set; }
    public int DailyWorkHours { get; set; }
}
