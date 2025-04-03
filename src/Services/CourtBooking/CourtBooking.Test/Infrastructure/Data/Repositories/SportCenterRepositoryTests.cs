using CourtBooking.Application.Data;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using CourtBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace CourtBooking.Test.Infrastructure.Data.Repositories
{
    [Collection("Sequential")]
    public class SportCenterRepositoryTests : IDisposable
    {
        private const string ConnectionString = "Host=localhost;Port=5432;Database=courtbooking_test;Username=postgres;Password=123456";
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public SportCenterRepositoryTests()
        {
            // Kết nối tới PostgreSQL để kiểm tra và tạo database nếu chưa tồn tại
            using var connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=postgres;Password=123456");
            connection.Open();
            using var command = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = 'courtbooking_test'", connection);
            var exists = command.ExecuteScalar() != null; // Kiểm tra database tồn tại
            if (!exists)
            {
                using var createCommand = new NpgsqlCommand("CREATE DATABASE courtbooking_test", connection);
                createCommand.ExecuteNonQuery();
            }

            // Cấu hình DbContextOptions
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

            // Áp dụng migrations và đảm bảo bảng tồn tại
            using var context = new ApplicationDbContext(_options);
            var isDatabaseCreated = context.Database.CanConnectAsync().Result;
            if (!isDatabaseCreated)
            {
                context.Database.MigrateAsync().Wait();
            }

            // Kiểm tra và tạo bảng thủ công nếu cần
            EnsureSportCentersTableExists(context);

            CleanTestData(context);
        }

        public void Dispose()
        {
            using var context = new ApplicationDbContext(_options);
            CleanTestData(context);
        }

        private ApplicationDbContext CreateContext()
            => new ApplicationDbContext(_options);

        private void EnsureSportCentersTableExists(ApplicationDbContext context)
        {
            var tableExists = context.Database
                .SqlQueryRaw<bool>("SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'sportcenters')")
                .FirstOrDefault();

            if (!tableExists)
            {
                // Tạo bảng "sportcenters" thủ công nếu migrations không tạo
                context.Database.ExecuteSqlRaw(@"
                    CREATE TABLE sportcenters (
                        id UUID PRIMARY KEY,
                        ownerid UUID NOT NULL,
                        name VARCHAR(100) NOT NULL,
                        phonenumber VARCHAR(11),
                        addressline VARCHAR(255) NOT NULL,
                        city VARCHAR(50),
                        district VARCHAR(50),
                        commune VARCHAR(50),
                        latitude DOUBLE PRECISION,
                        longitude DOUBLE PRECISION,
                        avatar VARCHAR(500),
                        imageurls JSONB,
                        description TEXT,
                        createdat TIMESTAMP NOT NULL,
                        lastmodified TIMESTAMP
                    );
                    CREATE SEQUENCE sportcenters_id_seq;
                ");
            }
        }

        private void CleanTestData(ApplicationDbContext context)
        {
            var tableExists = context.Database
                .SqlQueryRaw<bool>("SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'sportcenters')")
                .FirstOrDefault();

            if (tableExists)
            {
                context.Database.ExecuteSqlRaw("TRUNCATE TABLE sportcenters CASCADE");
                context.Database.ExecuteSqlRaw("ALTER SEQUENCE sportcenters_id_seq RESTART WITH 1");
            }
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
                Assert.Equal("Center 6", page2.First().Name);
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
                new Location("Main Street", city, "Vietnam", "700000"),
                new GeoLocation(10.762622, 106.660172),
                new SportCenterImages("main.jpg", new List<string> { "1.jpg", "2.jpg" }),
                "Test description"
            );
        }
    }
}