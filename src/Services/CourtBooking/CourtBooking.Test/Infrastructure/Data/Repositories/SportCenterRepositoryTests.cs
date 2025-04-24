using CourtBooking.Application.Data;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Infrastructure.Data;
using CourtBooking.Infrastructure.Data.Repositories;
using CourtBooking.Test.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Infrastructure.Data.Repositories
{
    [Collection("PostgresDatabase")]
    public class SportCenterRepositoryTests
    {
        private readonly PostgresTestFixture _fixture;

        public SportCenterRepositoryTests(PostgresTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task AddSportCenterAsync_Should_PersistDataCorrectly()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new SportCenterRepository(context);
            var ownerId = OwnerId.Of(Guid.NewGuid());
            var sportCenter = CreateTestCenter(ownerId, "Tennis Pro");

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // Act
                await repository.AddSportCenterAsync(sportCenter, CancellationToken.None);
                await context.SaveChangesAsync();

                // Assert
                var savedCenter = context.SportCenters.First();
                Assert.Equal(sportCenter.PhoneNumber, savedCenter.PhoneNumber);
            }
            finally
            {
                await transaction.RollbackAsync();
            }
        }

        [Fact]
        public async Task GetSportCenterByIdAsync_Should_ReturnCorrectCenter()
        {
            using var context = CreateContext();
            var repository = new SportCenterRepository(context);

            var ownerId = OwnerId.Of(Guid.NewGuid());

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var expectedCenter = await CreateAndSaveCenterInContext(context, ownerId, "Tennis Pro");

                var result = await repository.GetSportCenterByIdAsync(expectedCenter.Id, CancellationToken.None);

                Assert.NotNull(result);
                Assert.Equal(expectedCenter.Name, result.Name);
                Assert.Equal(expectedCenter.PhoneNumber, result.PhoneNumber);
            }
            finally
            {
                await transaction.RollbackAsync();
            }
        }

        [Fact]
        public async Task GetSportCentersByOwnerIdAsync_Should_OnlyReturnOwnedCenters()
        {
            using var context = CreateContext();
            var repository = new SportCenterRepository(context);

            var ownerId1 = Guid.NewGuid();
            var ownerId2 = Guid.NewGuid();

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await CreateAndSaveCenterInContext(context, OwnerId.Of(ownerId1), "Owner 1 Center A");
                await CreateAndSaveCenterInContext(context, OwnerId.Of(ownerId1), "Owner 1 Center B");
                await CreateAndSaveCenterInContext(context, OwnerId.Of(ownerId2), "Owner 2 Center");

                var results = await repository.GetSportCentersByOwnerIdAsync(ownerId1, CancellationToken.None);

                Assert.Equal(2, results.Count);
                Assert.All(results, sc => Assert.Equal(OwnerId.Of(ownerId1), sc.OwnerId));
            }
            finally
            {
                await transaction.RollbackAsync();
            }
        }

        [Fact]
        public async Task UpdateSportCenterAsync_Should_UpdateCorrectly()
        {
            using var context = CreateContext();
            var repository = new SportCenterRepository(context);

            var ownerId = OwnerId.Of(Guid.NewGuid());
            var originalName = "Original Name";
            var updatedName = "Updated Name";

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var center = await CreateAndSaveCenterInContext(context, ownerId, originalName);

                center.UpdateInfo(updatedName, center.PhoneNumber, center.Description);
                await repository.UpdateSportCenterAsync(center, CancellationToken.None);

                var updatedCenter = await context.SportCenters.FindAsync(center.Id);
                Assert.Equal(updatedName, updatedCenter.Name);
            }
            finally
            {
                await transaction.RollbackAsync();
            }
        }

        [Fact]
        public async Task GetPaginatedAsync_Should_ReturnCorrectPageSize()
        {
            using var context = CreateContext();
            var repository = new SportCenterRepository(context);
            var ownerId = OwnerId.Of(Guid.NewGuid());

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                for (int i = 1; i <= 15; i++)
                {
                    await CreateAndSaveCenterInContext(context, ownerId, $"Center {i}");
                }

                var page1 = await repository.GetPaginatedSportCentersAsync(0, 5, CancellationToken.None);
                var page2 = await repository.GetPaginatedSportCentersAsync(1, 5, CancellationToken.None);

                Assert.Equal(5, page1.Count);
                Assert.Equal(5, page2.Count);
                Assert.NotEqual(page1[0].Id, page2[0].Id);
            }
            finally
            {
                await transaction.RollbackAsync();
            }
        }

        [Fact]
        public async Task SearchByName_Should_SupportPartialMatches()
        {
            using var context = CreateContext();
            var repository = new SportCenterRepository(context);
            var ownerId = OwnerId.Of(Guid.NewGuid());

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var names = new[] { "Tennis Court", "Badminton Club", "Tennis Academy" };
                foreach (var name in names)
                {
                    await CreateAndSaveCenterInContext(context, ownerId, name);
                }

                var tennisResults = await repository.GetFilteredPaginatedSportCentersAsync(
                    0, 10, null, "Tennis", CancellationToken.None);
                Assert.Equal(2, tennisResults.Count);

                var clubResults = await repository.GetFilteredPaginatedSportCentersAsync(
                    0, 10, null, "Club", CancellationToken.None);
                Assert.Single(clubResults);
            }
            finally
            {
                await transaction.RollbackAsync();
            }
        }

        [Fact]
        public async Task CountMethods_Should_ReturnAccurateNumbers()
        {
            using var context = CreateContext();
            var repository = new SportCenterRepository(context);
            var ownerId = OwnerId.Of(Guid.NewGuid());

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // Xóa tất cả các trung tâm hiện có
                foreach (var center in context.SportCenters)
                {
                    context.SportCenters.Remove(center);
                }
                await context.SaveChangesAsync();
                
                // Thêm 7 trung tâm mới
                for (int i = 1; i <= 7; i++)
                {
                    await CreateAndSaveCenterInContext(context, ownerId, $"Center {i}");
                }

                // Chỉ kiểm tra filteredCount
                var filteredCount = await repository.GetFilteredSportCenterCountAsync(
                    "HCMC", "Tennis", CancellationToken.None);
                Assert.Equal(0, filteredCount);
            }
            finally
            {
                await transaction.RollbackAsync();
            }
        }

        [Fact]
        public void Database_Should_HaveSportCentersTable()
        {
            // Arrange
            var ownerId = OwnerId.Of(Guid.NewGuid());
            var sportCenter = SportCenter.Create(
                SportCenterId.Of(Guid.NewGuid()),
                ownerId,
                "Test Sport Center",
                "0123456789",
                new Location("123 Main St", "HCMC", "Vietnam", "70000"),
                new GeoLocation(10.762622, 106.660172),
                new SportCenterImages("main.jpg", new List<string> { "1.jpg", "2.jpg" }),
                "A great sport center"
            );

            // Act
            using var context = CreateContext();
            context.SportCenters.Add(sportCenter);
            context.SaveChanges();

            // Assert
            var savedSportCenter = context.SportCenters.FirstOrDefault();
            Assert.NotNull(savedSportCenter);
            Assert.Equal("Test Sport Center", savedSportCenter.Name);
        }

        private ApplicationDbContext CreateContext()
        {
            return new ApplicationDbContext(_fixture.ContextOptions);
        }

        private SportCenter CreateTestCenter(
            OwnerId ownerId,
            string name = "Default Center",
            string city = "HCMC",
            string phone = "0123456789")
        {
            return SportCenter.Create(
                SportCenterId.Of(Guid.NewGuid()),
                ownerId,
                name,
                phone,
                new Location("123 Main St", city, "Vietnam", "70000"),
                new GeoLocation(10.762622, 106.660172),
                new SportCenterImages("main.jpg", new List<string> { "1.jpg" }),
                "Test description"
            );
        }

        private async Task<SportCenter> CreateAndSaveCenterInContext(
            ApplicationDbContext context,
            OwnerId ownerId,
            string name)
        {
            var center = CreateTestCenter(ownerId, name);
            context.SportCenters.Add(center);
            await context.SaveChangesAsync();
            return center;
        }
    }
}