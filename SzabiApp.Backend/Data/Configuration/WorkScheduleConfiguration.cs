using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SzabiApp.Backend.Models.Entities;

namespace SzabiApp.Backend.Data.Configuration;

public class WorkScheduleConfiguration : IEntityTypeConfiguration<WorkSchedule>
{
    public void Configure(EntityTypeBuilder<WorkSchedule> builder)
    {
        builder.HasKey(ws => ws.Id);

        builder.Property(ws => ws.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ws => ws.WorkDaysPerWeek)
            .IsRequired();

        builder.Property(ws => ws.DailyWorkHours)
            .IsRequired();

        builder.ToTable("work_schedules");
    }
}
