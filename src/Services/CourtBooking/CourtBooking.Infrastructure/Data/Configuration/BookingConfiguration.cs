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

            builder.Property(b => b.BookingDate)
                .HasColumnType("DATE")
                .IsRequired();

            builder.Property(b => b.TotalTime)
                .HasColumnType("DECIMAL")
                .IsRequired();

            builder.Property(b => b.TotalPrice)
                .HasColumnType("DECIMAL")
                .IsRequired();

            builder.Property(b => b.Status)
                .HasConversion(
                    status => (int)status,
                    value => (BookingStatus)value)
                .IsRequired();

            builder.Property(b => b.Note)
                .HasColumnType("TEXT")
                .IsRequired(false);

            builder.HasMany(b => b.BookingDetails)
                .WithOne()
                .HasForeignKey(bp => bp.BookingId)
                .IsRequired();
        }
    }
}
