using CourtBooking.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourtBooking.Infrastructure.Data.Configuration
{
    public class CourtPromotionConfiguration : IEntityTypeConfiguration<CourtPromotion>
    {
        public void Configure(EntityTypeBuilder<CourtPromotion> builder)
        {
            builder.ToTable("court_promotions");

            builder.HasKey(cp => cp.Id);

            builder.Property(cp => cp.Id)
                .HasConversion(
                    id => id.Value,
                    value => CourtPromotionId.Of(value))
                .IsRequired();

            builder.Property(cp => cp.CourtId)
                .HasConversion(
                    id => id.Value,
                    value => CourtId.Of(value))
                .IsRequired();

            builder.Property(cp => cp.Description)
                .HasColumnType("TEXT")
                .IsRequired(false);

            builder.Property(cp => cp.DiscountType)
                .HasColumnType("VARCHAR(50)")
                .IsRequired();

            builder.Property(cp => cp.DiscountValue)
                .HasColumnType("DECIMAL")
                .IsRequired();

            builder.Property(cp => cp.ValidFrom)
                .HasColumnType("DATE")
                .IsRequired();

            builder.Property(cp => cp.ValidTo)
                .HasColumnType("DATE")
                .IsRequired();

            builder.HasOne<Court>()
                .WithMany()
                .HasForeignKey(cp => cp.CourtId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
