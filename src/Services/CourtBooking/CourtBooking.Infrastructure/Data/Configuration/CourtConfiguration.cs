using CourtBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourtBooking.Infrastructure.Data.Configuration;

public class CourtConfiguration : IEntityTypeConfiguration<Court>
{
    public void Configure(EntityTypeBuilder<Court> builder)
    {
        builder.ToTable("courts");

        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => CourtId.Of(value));

        builder.ComplexProperty(
             c => c.CourtName, courtNameBuilder =>
             {
                 courtNameBuilder.Property(n => n.Value)
                     .HasColumnName(nameof(Court.CourtName))
                     .HasMaxLength(255)
                     .IsRequired();
             });

        builder.Property(c => c.SportCenterId)
            .HasConversion(
                id => id.Value,
                value => SportCenterId.Of(value))
            .IsRequired();

        builder.Property(c => c.SportId)
            .HasConversion(
                id => id.Value,
                value => SportId.Of(value))
            .IsRequired();

        builder.Property(c => c.SlotDuration)
            .HasConversion(
                duration => duration.TotalMinutes, 
                minutes => TimeSpan.FromMinutes(minutes))
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnType("TEXT");

        builder.Property(c => c.Facilities)
            .HasColumnType("JSONB");
        
        builder.Property(c => c.Status)
            .HasDefaultValue(CourtStatus.Open)
            .HasConversion(builder => builder.ToString(), 
            value => (CourtStatus)Enum.Parse(typeof(CourtStatus), value));

        builder.HasMany(c => c.CourtSlots)
            .WithOne()
            .HasForeignKey(c => c.CourtId)
            .IsRequired();
    }
}