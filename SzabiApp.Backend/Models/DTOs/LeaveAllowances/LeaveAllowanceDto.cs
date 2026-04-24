using SzabiApp.Backend.Models.Enums;

namespace SzabiApp.Backend.Models.DTOs.LeaveAllowances;

public class LeaveAllowanceDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public int Year { get; set; }
    public LeaveCategory Category { get; set; }
    public int TotalDays { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays { get; set; }
}
