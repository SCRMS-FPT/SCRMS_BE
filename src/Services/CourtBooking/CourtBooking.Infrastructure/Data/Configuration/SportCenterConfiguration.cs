using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace CourtBooking.Infrastructure.Data.Configuration;
public class SportCenterConfiguration : IEntityTypeConfiguration<SportCenter>
{
    public void Configure(EntityTypeBuilder<SportCenter> builder)
    {
        builder.ToTable("sport_centers");

        builder.HasKey(sc => sc.Id);


        builder.Property(sc => sc.Id)
            .HasConversion(
                id => id.Value,
                value => SportCenterId.Of(value))
            .IsRequired();

        builder.Property(sc => sc.OwnerId)
        .HasConversion(
            id => id.Value,
            value => OwnerId.Of(value))
            .IsRequired();

        builder.Property(sc => sc.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(sc => sc.PhoneNumber)
            .HasMaxLength(11);

        builder.ComplexProperty(
         c => c.Address, locationBuilder =>
         {
             locationBuilder.Property(l => l.AddressLine)
             .HasMaxLength(255)
             .IsRequired();

             locationBuilder.Property(l => l.City)
             .HasMaxLength(50);

             locationBuilder.Property(l => l.District)
             .HasMaxLength(50);

             locationBuilder.Property(l => l.Commune)
             .HasMaxLength(50);
         });

        builder.ComplexProperty(sc => sc.LocationPoint, location =>
        {
            location.Property(l => l.Latitude)
                .HasColumnType("DOUBLE PRECISION");
            location.Property(l => l.Longitude)
                .HasColumnType("DOUBLE PRECISION");
        });

        builder.OwnsOne(sc => sc.Images, images =>
        {
            images.Property(i => i.Avatar)
                .HasMaxLength(500);

            images.Property(i => i.ImageUrls)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                );
        });

        builder.Property(sc => sc.Description)
            .HasColumnType("TEXT")
            .IsRequired(false);


        builder.Property(sc => sc.IsDeleted)
        .HasColumnName("is_deleted")
        .HasDefaultValue(false)
        .IsRequired();

        builder.HasMany(sc => sc.Courts)
        .WithOne()
        .HasForeignKey(c => c.SportCenterId)
        .IsRequired();

    }
}

