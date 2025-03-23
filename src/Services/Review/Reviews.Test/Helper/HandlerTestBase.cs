using Microsoft.EntityFrameworkCore;
using Reviews.API.Data;
using Reviews.API.Data.Repositories;

namespace Reviews.Test.Helper
{
    public abstract class HandlerTestBase : IDisposable
    {
        protected ReviewDbContext Context { get; }
        protected ReviewRepository Repository { get; }
        protected DateTime Now => DateTime.UtcNow;
        protected HandlerTestBase()
        {
            var options = new DbContextOptionsBuilder<ReviewDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            Context = new ReviewDbContext(options);
            Repository = new ReviewRepository(Context);
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}
