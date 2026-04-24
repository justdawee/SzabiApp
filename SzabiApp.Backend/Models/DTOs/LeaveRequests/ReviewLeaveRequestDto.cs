using SzabiApp.Backend.Models.Enums;

namespace SzabiApp.Backend.Models.DTOs.LeaveRequests;

public class ReviewLeaveRequestDto
{
    public LeaveStatus Decision { get; set; }
    public string? ReviewNote { get; set; }
}
