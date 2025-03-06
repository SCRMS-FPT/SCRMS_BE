using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourtBooking.Infrastructure.Data.Configuration
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("bookings");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                .HasConversion(
                    id => id.Value,
                    value => BookingId.Of(value))
                .IsRequired();

            builder.Property(b => b.UserId)
                .HasConversion(
                    id => id.Value,
                    value => UserId.Of(value))
                .IsRequired();

            builder.Property(b => b.CourtId)
                .HasConversion(
                    id => id.Value,
                    value => CourtId.Of(value))
                .IsRequired();

            builder.Property(b => b.BookingDate)
                .HasColumnType("DATE")
                .IsRequired();

            builder.Property(b => b.StartTime)
                .HasColumnType("TIME")
                .IsRequired();

            builder.Property(b => b.EndTime)
                .HasColumnType("TIME")
                .IsRequired();

            builder.Property(b => b.TotalPrice)
                .HasColumnType("DECIMAL")
                .IsRequired();

            builder.Property(b => b.Status)
                .HasConversion(
                    status => (int)status,
                    value => (BookingStatus)value)
                .IsRequired();

            builder.Property(b => b.PromotionId)
                .HasConversion(
                    id => id.Value,
                    value => PromotionId.Of(value))
                .IsRequired(false);

            builder.HasMany(b => b.BookingPrices)
                .WithOne()
                .HasForeignKey(bp => bp.BookingId)
                .IsRequired();
        }
    }
}
