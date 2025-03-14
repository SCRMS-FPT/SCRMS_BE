using Identity.Application.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Identity.Test.Helpers
{
    public static class TestDbContextFactory
    {
        public static IdentityDbContext Create()
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new IdentityDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}