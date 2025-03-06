// File: ServicePackageRepositoryTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Identity.Application.Data.Repositories;
using Identity.Domain.Models;
using Identity.Test.Fakes;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Identity.Test.RepositoryTests
{
    public class ServicePackageRepositoryTests
    {
        private DbContextOptions<IdentityDbContext> GetOptions(string dbName) =>
            new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

        [Fact]
        public async Task GetServicePackageByIdAsync_ReturnsPackage_WhenExists()
        {
            // Arrange
            var options = GetOptions("ServicePackage_GetByIdTest");
            using var context = new IdentityDbContext(options);
            var package = ServicePackage.Create("Test Package", "Description", 100.00m, 30, "Basic");
            context.ServicePackages.Add(package);
            await context.SaveChangesAsync();
            var repo = new FakeServicePackageRepository(context);

            // Act
            var result = await repo.GetServicePackageByIdAsync(package.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(package.Id);
        }

        [Fact]
        public async Task GetServicePackageByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var options = GetOptions("ServicePackage_GetById_NotFound");
            using var context = new IdentityDbContext(options);
            var repo = new FakeServicePackageRepository(context);

            // Act
            var result = await repo.GetServicePackageByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllServicePackageAsync_ReturnsAllPackages_WhenExist()
        {
            // Arrange
            var options = GetOptions("ServicePackage_GetAllTest");
            using var context = new IdentityDbContext(options);
            var packages = new List<ServicePackage>
            {
                ServicePackage.Create("Package1", "Desc1", 50.00m, 30, "Basic"),
                ServicePackage.Create("Package2", "Desc2", 100.00m, 60, "Premium")
            };
            context.ServicePackages.AddRange(packages);
            await context.SaveChangesAsync();
            var repo = new FakeServicePackageRepository(context);

            // Act
            var result = await repo.GetAllServicePackageAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllServicePackageAsync_ReturnsEmptyList_WhenNoneExist()
        {
            // Arrange
            var options = GetOptions("ServicePackage_GetAll_Empty");
            using var context = new IdentityDbContext(options);
            var repo = new FakeServicePackageRepository(context);

            // Act
            var result = await repo.GetAllServicePackageAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AddServicePackageAsync_AddsPackageSuccessfully()
        {
            // Arrange
            var options = GetOptions("ServicePackage_AddTest");
            using var context = new IdentityDbContext(options);
            var repo = new FakeServicePackageRepository(context);
            var package = ServicePackage.Create("New Package", "Desc", 75.00m, 30, "Basic");

            // Act
            await repo.AddServicePackageAsync(package);

            // Assert
            var result = await context.ServicePackages.FindAsync(package.Id);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateServicePackageAsync_UpdatesPackageSuccessfully()
        {
            // Arrange
            var options = GetOptions("ServicePackage_UpdateTest");
            using var context = new IdentityDbContext(options);
            var package = ServicePackage.Create("Old Package", "Old Desc", 50.00m, 30, "Basic");
            context.ServicePackages.Add(package);
            await context.SaveChangesAsync();
            var repo = new FakeServicePackageRepository(context);

            // Act
            package.UpdateDetails("Updated Package", "New Desc", 60.00m, 45, "Basic", "active");
            await repo.UpdateServicePackageAsync(package);

            // Assert
            var updated = await context.ServicePackages.FindAsync(package.Id);
            updated.Name.Should().Be("Updated Package");
            updated.Price.Should().Be(60.00m);
        }

        [Fact]
        public async Task DeleteServicePackageAsync_DeletesPackageSuccessfully()
        {
            // Arrange
            var options = GetOptions("ServicePackage_DeleteTest");
            using var context = new IdentityDbContext(options);
            var package = ServicePackage.Create("Package To Delete", "Desc", 50.00m, 30, "Basic");
            context.ServicePackages.Add(package);
            await context.SaveChangesAsync();
            var repo = new FakeServicePackageRepository(context);

            // Act
            await repo.DeleteServicePackageAsync(package);

            // Assert
            var result = await context.ServicePackages.FindAsync(package.Id);
            result.Should().BeNull();
        }
    }
}