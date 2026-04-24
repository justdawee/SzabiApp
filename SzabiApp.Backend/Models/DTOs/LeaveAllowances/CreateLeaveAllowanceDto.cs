using SzabiApp.Backend.Models.Enums;

namespace SzabiApp.Backend.Models.DTOs.LeaveAllowances;

public class CreateLeaveAllowanceDto
{
    public Guid UserId { get; set; }
    public int Year { get; set; }
    public LeaveCategory Category { get; set; }
    public int TotalDays { get; set; }
}
