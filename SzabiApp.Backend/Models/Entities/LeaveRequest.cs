using NodaTime;
using SzabiApp.Backend.Models.Enums;

namespace SzabiApp.Backend.Models.Entities;

public class LeaveRequest
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public LeaveCategory Category { get; set; }
    public LocalDate StartDate { get; set; }
    public LocalDate EndDate { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    public string? RequestNote { get; set; }
    public string? ReviewNote { get; set; }

    public Guid? ReviewedById { get; set; }
    public User? ReviewedBy { get; set; }
    public LocalDateTime? ReviewedAt { get; set; }

    public LocalDateTime CreatedAt { get; set; }
}
