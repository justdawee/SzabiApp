using Riok.Mapperly.Abstractions;
using SzabiApp.Backend.Models.DTOs.Holidays;
using SzabiApp.Backend.Models.DTOs.LeaveAllowances;
using SzabiApp.Backend.Models.DTOs.LeaveRequests;
using SzabiApp.Backend.Models.DTOs.Users;
using SzabiApp.Backend.Models.DTOs.WorkSchedules;
using SzabiApp.Backend.Models.Entities;

namespace SzabiApp.Backend.Mappings;

[Mapper]
public partial class AppMapper
{
    // --- User ---

    [MapperIgnoreTarget(nameof(UserDto.ManagerFullName))]
    [MapperIgnoreTarget(nameof(UserDto.WorkScheduleName))]
    private partial UserDto UserToDtoCore(User user);

    public UserDto UserToDto(User user)
    {
        var dto = UserToDtoCore(user);
        dto.ManagerFullName = user.Manager is null
            ? null
            : $"{user.Manager.LastName} {user.Manager.FirstName}";
        dto.WorkScheduleName = user.WorkSchedule?.Name;
        return dto;
    }

    // --- LeaveRequest ---

    [MapperIgnoreTarget(nameof(LeaveRequestDto.UserFullName))]
    [MapperIgnoreTarget(nameof(LeaveRequestDto.ReviewedByFullName))]
    private partial LeaveRequestDto LeaveRequestToDtoCore(LeaveRequest leaveRequest);

    public LeaveRequestDto LeaveRequestToDto(LeaveRequest leaveRequest)
    {
        var dto = LeaveRequestToDtoCore(leaveRequest);
        dto.UserFullName = $"{leaveRequest.User.LastName} {leaveRequest.User.FirstName}";
        dto.ReviewedByFullName = leaveRequest.ReviewedBy is null
            ? null
            : $"{leaveRequest.ReviewedBy.LastName} {leaveRequest.ReviewedBy.FirstName}";
        return dto;
    }

    // --- Holiday ---

    public partial HolidayDto HolidayToDto(Holiday holiday);

    // --- WorkSchedule ---

    public partial WorkScheduleDto WorkScheduleToDto(WorkSchedule workSchedule);

    // --- LeaveAllowance ---

    [MapperIgnoreTarget(nameof(LeaveAllowanceDto.UserFullName))]
    [MapperIgnoreTarget(nameof(LeaveAllowanceDto.RemainingDays))]
    private partial LeaveAllowanceDto LeaveAllowanceToDtoCore(LeaveAllowance allowance);

    public LeaveAllowanceDto LeaveAllowanceToDto(LeaveAllowance allowance)
    {
        var dto = LeaveAllowanceToDtoCore(allowance);
        dto.UserFullName = $"{allowance.User.LastName} {allowance.User.FirstName}";
        dto.RemainingDays = allowance.RemainingDays;
        return dto;
    }

    // --- Collections ---

    public IEnumerable<UserDto> UsersToDto(IEnumerable<User> users) =>
        users.Select(UserToDto);

    public IEnumerable<LeaveRequestDto> LeaveRequestsToDto(IEnumerable<LeaveRequest> leaveRequests) =>
        leaveRequests.Select(LeaveRequestToDto);

    public IEnumerable<WorkScheduleDto> WorkSchedulesToDto(IEnumerable<WorkSchedule> schedules) =>
        schedules.Select(WorkScheduleToDto);

    public IEnumerable<LeaveAllowanceDto> LeaveAllowancesToDto(IEnumerable<LeaveAllowance> allowances) =>
        allowances.Select(LeaveAllowanceToDto);
}
