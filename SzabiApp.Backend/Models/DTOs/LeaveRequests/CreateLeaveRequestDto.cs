using NodaTime;
using SzabiApp.Backend.Models.Enums;

namespace SzabiApp.Backend.Models.DTOs.LeaveRequests;

public class CreateLeaveRequestDto
{
    public LeaveCategory Category { get; set; }
    public LocalDate StartDate { get; set; }
    public LocalDate EndDate { get; set; }
    public string? RequestNote { get; set; }
}
