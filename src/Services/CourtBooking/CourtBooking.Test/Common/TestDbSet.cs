using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CourtBooking.Test.Common
{
    public class TestDbSet<T> : DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>, IEnumerable<T> where T : class
    {
        private readonly List<T> _data;
        private readonly IQueryable<T> _query;

        public TestDbSet()
        {
            _data = new List<T>();
            _query = _data.AsQueryable().AsAsyncQueryable();
        }

        public TestDbSet(IEnumerable<T> data)
        {
            _data = new List<T>(data);
            _query = _data.AsQueryable().AsAsyncQueryable();
        }

        public override IEntityType EntityType => throw new NotImplementedException();

        #region DbSet Implementation
        public override EntityEntry<T> Add(T entity)
        {
            _data.Add(entity);
            return null; // Mock implementation không cần trả về EntityEntry<T> thực
        }

        public override ValueTask<EntityEntry<T>> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            _data.Add(entity);
            return new ValueTask<EntityEntry<T>>((EntityEntry<T>)null); // Truyền trực tiếp null với cast
        }

        public override void AddRange(params T[] entities)
        {
            _data.AddRange(entities);
        }

        public override Task AddRangeAsync(params T[] entities)
        {
            _data.AddRange(entities);
            return Task.CompletedTask;
        }

        public override void AddRange(IEnumerable<T> entities)
        {
            _data.AddRange(entities);
        }

        public override Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            _data.AddRange(entities);
            return Task.CompletedTask;
        }

        public override EntityEntry<T> Remove(T entity)
        {
            _data.Remove(entity);
            return null; // Mock implementation
        }

        public override void RemoveRange(params T[] entities)
        {
            foreach (var entity in entities)
            {
                _data.Remove(entity);
            }
        }

        public override void RemoveRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                _data.Remove(entity);
            }
        }

        public override EntityEntry<T> Update(T entity)
        {
            return null; // Mock implementation
        }

        public override void UpdateRange(params T[] entities)
        {
            // No-op for inmemory implementation
        }

        public override void UpdateRange(IEnumerable<T> entities)
        {
            // No-op for inmemory implementation
        }
        #endregion

        #region IQueryable Implementation
        public IEnumerator<T> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public Type ElementType => _query.ElementType;

        public Expression Expression => _query.Expression;

        public IQueryProvider Provider => _query.Provider;

        public override IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(_data.GetEnumerator());
        }
        #endregion
    }
} 