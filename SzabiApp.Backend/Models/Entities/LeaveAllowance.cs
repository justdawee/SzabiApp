using SzabiApp.Backend.Models.Enums;

namespace SzabiApp.Backend.Models.Entities;

public class LeaveAllowance
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public int Year { get; set; }
    public LeaveCategory Category { get; set; }
    public int TotalDays { get; set; }
    public int UsedDays { get; set; }

    public int RemainingDays => TotalDays - UsedDays;
}
