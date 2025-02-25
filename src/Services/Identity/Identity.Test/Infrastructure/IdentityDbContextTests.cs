using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Test.Infrastructure
{
    public class IdentityDbContextTests
    {
        private DbContextOptions<IdentityDbContext> GetInMemoryOptions(string dbName) => new DbContextOptionsBuilder<IdentityDbContext>().UseInMemoryDatabase(databaseName: dbName).Options;

        [Fact]
        public async Task CanAddAndRetrieveUser()
        {
            var options = GetInMemoryOptions("UserTestDb");
            using var context = new IdentityDbContext(options);

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = "test@example.com",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                Gender = Gender.Male,
                CreatedAt = DateTime.UtcNow
            };

            context.Add(user);
            await context.SaveChangesAsync();

            var retrieved = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            retrieved.Should().NotBeNull();
            retrieved.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task CanAddAndRetrieveServicePackage()
        {
            var options = GetInMemoryOptions("ServicePackageTestDb");
            using var context = new IdentityDbContext(options);

            var package = ServicePackage.Create("Test Package", "Description", 100, 30, "Basic");
            context.ServicePackages.Add(package);
            await context.SaveChangesAsync();

            var retrieved = await context.ServicePackages.FirstOrDefaultAsync(p => p.Id == package.Id);
            retrieved.Should().NotBeNull();
            retrieved.Name.Should().Be("Test Package");
        }
    }
}