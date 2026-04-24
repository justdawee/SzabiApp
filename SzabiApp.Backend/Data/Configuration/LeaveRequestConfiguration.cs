using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SzabiApp.Backend.Models.Entities;

namespace SzabiApp.Backend.Data.Configuration;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.HasKey(lr => lr.Id);

        builder.Property(lr => lr.Category)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(lr => lr.StartDate)
            .IsRequired();

        builder.Property(lr => lr.EndDate)
            .IsRequired();

        builder.Property(lr => lr.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(Models.Enums.LeaveStatus.Pending);

        builder.Property(lr => lr.RequestNote)
            .HasMaxLength(1000);

        builder.Property(lr => lr.ReviewNote)
            .HasMaxLength(1000);

        builder.Property(lr => lr.CreatedAt)
            .IsRequired();

        builder.HasOne(lr => lr.User)
            .WithMany(u => u.LeaveRequests)
            .HasForeignKey(lr => lr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lr => lr.ReviewedBy)
            .WithMany(u => u.ReviewedRequests)
            .HasForeignKey(lr => lr.ReviewedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("leave_requests");
    }
}
