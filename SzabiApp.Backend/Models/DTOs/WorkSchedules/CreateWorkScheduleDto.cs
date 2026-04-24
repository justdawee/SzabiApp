namespace SzabiApp.Backend.Models.DTOs.WorkSchedules;

public class CreateWorkScheduleDto
{
    public string Name { get; set; } = string.Empty;
    public int WorkDaysPerWeek { get; set; }
    public int DailyWorkHours { get; set; }
}
