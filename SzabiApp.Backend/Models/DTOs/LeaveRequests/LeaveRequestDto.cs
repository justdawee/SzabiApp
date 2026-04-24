using NodaTime;
using SzabiApp.Backend.Models.Enums;

namespace SzabiApp.Backend.Models.DTOs.LeaveRequests;

public class LeaveRequestDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public LeaveCategory Category { get; set; }
    public LocalDate StartDate { get; set; }
    public LocalDate EndDate { get; set; }
    public LeaveStatus Status { get; set; }
    public string? RequestNote { get; set; }
    public string? ReviewNote { get; set; }
    public Guid? ReviewedById { get; set; }
    public string? ReviewedByFullName { get; set; }
    public LocalDateTime? ReviewedAt { get; set; }
    public LocalDateTime CreatedAt { get; set; }
}
