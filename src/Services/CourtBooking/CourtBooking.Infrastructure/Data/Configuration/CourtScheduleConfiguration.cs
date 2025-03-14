using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq;

namespace CourtBooking.Infrastructure.Data.Configuration
{
    public class CourtScheduleConfiguration : IEntityTypeConfiguration<CourtSchedule>
    {
        public void Configure(EntityTypeBuilder<CourtSchedule> builder)
        {
            builder.ToTable("court_schedules");

            builder.HasKey(cs => cs.Id);

            builder.Property(cs => cs.Id)
                .HasConversion(
                    id => id.Value,
                    value => CourtScheduleId.Of(value));

            builder.Property(cs => cs.CourtId)
                .HasConversion(
                    id => id.Value,
                    value => CourtId.Of(value))
                .IsRequired();

            builder.Property(cs => cs.DayOfWeek)
                .HasColumnType("integer[]")
                .HasConversion(
                    days => days.Days.ToArray(),
                    value => new DayOfWeekValue(value))
                .IsRequired();

            builder.Property(cs => cs.StartTime)
                .HasColumnType("TIME")
                .IsRequired();

            builder.Property(cs => cs.EndTime)
                .HasColumnType("TIME")
                .IsRequired();

            builder.Property(cs => cs.PriceSlot)
                .HasColumnType("DECIMAL")
                .IsRequired();

            builder.Property(cs => cs.Status)
                .HasConversion(
                    status => (int)status,
                    value => (CourtScheduleStatus)value)
                .IsRequired();
            builder.HasIndex(x => new { x.CourtId, x.DayOfWeek });
        }
    }
}