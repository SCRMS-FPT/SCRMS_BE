using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourtBooking.Infrastructure.Data.Configuration
{
    public class CourtOperatingHourConfiguration : IEntityTypeConfiguration<CourtOperatingHour>
    {
        public void Configure(EntityTypeBuilder<CourtOperatingHour> builder)
        {
            builder.ToTable("court_operating_hours");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasConversion(
                    id => id.Value,
                    value =>  CourtOperatingHourId.Of(value));

        }
    }
}
