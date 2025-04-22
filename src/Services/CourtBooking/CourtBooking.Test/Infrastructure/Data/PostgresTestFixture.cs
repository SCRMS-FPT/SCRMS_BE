using CourtBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CourtBooking.Test.Infrastructure.Data
{
    public class PostgresTestFixture : IDisposable, IAsyncLifetime
    {
        private const string ConnectionString = "Host=localhost;Port=5432;Database=courtbooking_test;Username=postgres;Password=123456";
        private readonly DbContextOptions<ApplicationDbContext> _contextOptions;
        private bool _databaseInitialized;

        public DbContextOptions<ApplicationDbContext> ContextOptions => _contextOptions;

        public PostgresTestFixture()
        {
            _contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(ConnectionString)
                .EnableSensitiveDataLogging()
                .Options;
        }

        public async Task InitializeAsync()
        {
            if (_databaseInitialized)
                return;

            using var context = new ApplicationDbContext(_contextOptions);

            // Ensure database is deleted and recreated
            await context.Database.EnsureDeletedAsync();

            // Create database without applying migrations
            await context.Database.EnsureCreatedAsync();

            _databaseInitialized = true;
        }

        public async Task DisposeAsync()
        {
            using var context = new ApplicationDbContext(_contextOptions);
            await context.Database.EnsureDeletedAsync();
            _databaseInitialized = false;
        }

        public void Dispose()
        {
            // Additional cleanup if needed
        }

        public ApplicationDbContext CreateContext()
        {
            var context = new ApplicationDbContext(_contextOptions);
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return context;
        }
    }

    [CollectionDefinition("PostgresDatabase")]
    public class DatabaseCollection : ICollectionFixture<PostgresTestFixture>
    {
        // This class has no code, and is never created.
        // Its purpose is to be the place to apply [CollectionDefinition] and all the ICollectionFixture<> interfaces.
    }
}