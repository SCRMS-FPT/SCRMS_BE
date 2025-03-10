using Matching.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Matching.Test.Helper
{
    public abstract class HandlerTestBase : IDisposable
    {
        protected readonly MatchingDbContext Context;

        protected HandlerTestBase()
        {
            var options = new DbContextOptionsBuilder<MatchingDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDB_{Guid.NewGuid()}")
                .Options;
            Context = new MatchingDbContext(options);
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}
