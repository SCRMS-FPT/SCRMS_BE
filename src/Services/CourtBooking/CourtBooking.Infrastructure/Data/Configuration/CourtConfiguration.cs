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

        builder.Property(c => c.OwnerId)
            .HasConversion(
                id => id.Value,
                value => OwnerId.Of(value));

        builder.HasMany(c => c.OperatingHours)
            .WithOne()
            .HasForeignKey(c => c.CourtId)
            .IsRequired();

         builder.HasOne(c => c.Sport)
                .WithMany()
                .HasForeignKey(c => c.SportId)
                .IsRequired();

        builder.ComplexProperty(
            c => c.Location, locationBuilder =>
            {
                locationBuilder.Property(l => l.Address)
                .HasMaxLength(255)
                .IsRequired();

                locationBuilder.Property(l => l.City)
                .HasMaxLength(50);

                locationBuilder.Property(l => l.District)
                .HasMaxLength(50);

                locationBuilder.Property(l => l.Commune)
                .HasMaxLength(50);
            });

        builder.Property(c => c.Description)
            .HasColumnType("TEXT");

        builder.Property(c => c.Facilities)
            .HasColumnType("JSON");
        
        builder.Property(c => c.Status)
            .HasDefaultValue(CourtStatus.Open)
            .HasConversion(builder => builder.ToString(), 
            value => (CourtStatus)Enum.Parse(typeof(CourtStatus), value));    

    }
}