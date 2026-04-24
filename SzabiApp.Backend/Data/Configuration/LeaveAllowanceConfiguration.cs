using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SzabiApp.Backend.Models.Entities;

namespace SzabiApp.Backend.Data.Configuration;

public class LeaveAllowanceConfiguration : IEntityTypeConfiguration<LeaveAllowance>
{
    public void Configure(EntityTypeBuilder<LeaveAllowance> builder)
    {
        builder.HasKey(la => la.Id);

        builder.Property(la => la.Year)
            .IsRequired();

        builder.Property(la => la.Category)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(la => la.TotalDays)
            .IsRequired();

        builder.Property(la => la.UsedDays)
            .IsRequired()
            .HasDefaultValue(0);

        // RemainingDays is computed — not mapped to a column
        builder.Ignore(la => la.RemainingDays);

        builder.HasIndex(la => new { la.UserId, la.Year, la.Category })
            .IsUnique();

        builder.HasOne(la => la.User)
            .WithMany(u => u.LeaveAllowances)
            .HasForeignKey(la => la.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("leave_allowances");
    }
}
