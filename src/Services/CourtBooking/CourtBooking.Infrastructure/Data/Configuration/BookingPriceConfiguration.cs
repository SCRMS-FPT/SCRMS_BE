using CourtBooking.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourtBooking.Infrastructure.Data.Configuration
{
    public class BookingPriceConfiguration : IEntityTypeConfiguration<BookingPrice>
    {
        public void Configure(EntityTypeBuilder<BookingPrice> builder)
        {
            builder.ToTable("booking_prices");

            builder.HasKey(bp => bp.Id);

            builder.Property(bp => bp.Id)
                .HasConversion(
                    id => id.Value,
                    value => BookingPriceId.Of(value))
                .IsRequired();

            builder.Property(bp => bp.BookingId)
                .HasConversion(
                    id => id.Value,
                    value => BookingId.Of(value))
                .IsRequired();

            builder.Property(bp => bp.StartTime)
                .HasColumnType("TIME")
                .IsRequired();

            builder.Property(bp => bp.EndTime)
                .HasColumnType("TIME")
                .IsRequired();

            builder.Property(bp => bp.Price)
                .HasColumnType("DECIMAL")
                .IsRequired();
        }
    }
}
