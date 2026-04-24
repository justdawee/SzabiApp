using NodaTime;
using SzabiApp.Backend.Models.Enums;

namespace SzabiApp.Backend.Models.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public LocalDate BirthDate { get; set; }
    public LocalDateTime CreatedAt { get; set; }

    public Guid? ManagerId { get; set; }
    public User? Manager { get; set; }

    public Guid? WorkScheduleId { get; set; }
    public WorkSchedule? WorkSchedule { get; set; }

    public ICollection<User> Subordinates { get; set; } = [];
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = [];
    public ICollection<LeaveRequest> ReviewedRequests { get; set; } = [];
    public ICollection<LeaveAllowance> LeaveAllowances { get; set; } = [];
}
