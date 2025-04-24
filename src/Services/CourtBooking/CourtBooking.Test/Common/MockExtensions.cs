using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Language.Flow;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CourtBooking.Test.Common
{
    public static class MockExtensions
    {
        public static void Setup<TEntity, TProperty>(
            Mock<DbSet<TEntity>> mockSet,
            Expression<Func<DbSet<TEntity>, IOrderedQueryable<TEntity>>> expression,
            IOrderedQueryable<TEntity> result) where TEntity : class
        {
            mockSet
                .Setup(expression)
                .Returns(result);
        }
    }
} 