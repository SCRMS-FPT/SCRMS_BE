using Identity.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.FirstName).HasMaxLength(255).IsRequired();
            builder.Property(u => u.LastName).HasMaxLength(255).IsRequired();
            builder.Property(u => u.BirthDate).IsRequired();
            builder.Property(u => u.Gender).HasConversion<string>().IsRequired();
        }
    }
}