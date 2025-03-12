using CourtBooking.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourtBooking.Infrastructure.Data.Configuration
{
    public class BookingDetailConfiguration : IEntityTypeConfiguration<BookingDetail>
    {
        public void Configure(EntityTypeBuilder<BookingDetail> builder)
        {
            builder.ToTable("booking_details");

            builder.HasKey(bp => bp.Id);

            builder.Property(bp => bp.Id)
                .HasConversion(
                    id => id.Value,
                    value => BookingDetailId.Of(value))
                .IsRequired();

            builder.Property(bp => bp.BookingId)
                .HasConversion(
                    id => id.Value,
                    value => BookingId.Of(value))
                .IsRequired();

            builder.Property(bp => bp.CourtId)
                .HasConversion(
                    id => id.Value,
                    value => CourtId.Of(value))
                .IsRequired();

            builder.Property(bp => bp.StartTime)
                .HasColumnType("TIME")
                .IsRequired();

            builder.Property(bp => bp.EndTime)
                .HasColumnType("TIME")
                .IsRequired();

            builder.Property(bp => bp.TotalPrice)
                .HasColumnType("DECIMAL")
                .IsRequired();

            builder.HasOne<Court>()
            .WithMany()
            .HasForeignKey(c => c.CourtId)
            .IsRequired();
        }
    }
}
