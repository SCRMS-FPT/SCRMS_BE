using CourtBooking.Application.Data;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Infrastructure.Data;
using CourtBooking.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

namespace CourtBooking.Test.Infrastructure.Data.Repositories
{
    [Collection("Sequential")]
    public class SportCenterRepositoryTests : IDisposable
    {
        private DbContextOptions<ApplicationDbContext> _options;
        private const string ConnectionString = "Host=localhost;Port=5432;Database=courtbooking_test;Username=postgres;Password=123456";

        public SportCenterRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

            using var context = CreateContext();
            context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            using var context = CreateContext();
            context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task AddSportCenterAsync_Should_PersistDataCorrectly()
        {
            using var context = CreateContext();
            var repository = new SportCenterRepository(context);

            var ownerId = OwnerId.Of(Guid.NewGuid());
            var sportCenter = CreateTestCenter(ownerId, "Tennis Pro");

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await repository.AddSportCenterAsync(sportCenter, CancellationToken.None);
                await context.SaveChangesAsync();

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
                for (int i = 1; i <= 7; i++)
                {
                    await CreateAndSaveCenterInContext(context, ownerId, $"Center {i}");
                }

                var totalCount = await repository.GetTotalSportCenterCountAsync(CancellationToken.None);
                Assert.Equal(7, totalCount);

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
            using var context = CreateContext();
            var tableExists = context.Database.SqlQueryRaw<bool>(
                "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'sportcenters')"
            ).FirstOrDefault();

            Assert.True(tableExists);
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

        private ApplicationDbContext CreateContext()
        {
            return new ApplicationDbContext(_options);
        }
    }
}