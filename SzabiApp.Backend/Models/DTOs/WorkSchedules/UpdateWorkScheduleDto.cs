namespace SzabiApp.Backend.Models.DTOs.WorkSchedules;

public class UpdateWorkScheduleDto
{
    public string? Name { get; set; }
    public int? WorkDaysPerWeek { get; set; }
    public int? DailyWorkHours { get; set; }
}
