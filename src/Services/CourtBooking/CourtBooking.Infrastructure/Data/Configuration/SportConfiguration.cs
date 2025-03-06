
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourtBooking.Infrastructure.Data.Configuration
{
    internal class SportConfiguration : IEntityTypeConfiguration<Sport>
    {
        public void Configure(EntityTypeBuilder<Sport> builder)
        {
            builder.ToTable("sports");

            builder.HasKey(s => s.Id);
            
            builder.Property(s => s.Id)
                .HasConversion(
                    id => id.Value,
                    value => SportId.Of(value));

            builder.Property(s => s.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(s => s.Icon)
                .HasColumnType("TEXT");

            builder.Property(s => s.Description)
                .HasColumnType("TEXT")
                .IsRequired(false);
        }
    }
}
