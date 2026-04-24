using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SzabiApp.Backend.Models.Entities;

namespace SzabiApp.Backend.Data.Configuration;

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Date)
            .IsRequired();

        builder.Property(h => h.IsRecurringYearly)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(h => h.Date);

        builder.ToTable("holidays");
    }
}
