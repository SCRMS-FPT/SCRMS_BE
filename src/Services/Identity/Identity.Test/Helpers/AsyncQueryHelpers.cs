using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Test.Helpers
{
    public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            Type resultType = typeof(TResult).GetGenericArguments()[0];
            object result = _inner.Execute(expression);

            if (result == null)
            {
                var defaultResult = Task.FromResult((object)null);
                return (TResult)(object)defaultResult;
            }

            var task = Task.FromResult(result);
            return (TResult)(object)task;
        }
    }

    public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        private readonly IQueryProvider _provider;

        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        {
            _provider = new TestAsyncQueryProvider<T>(this.AsQueryable().Provider);
        }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        {
            _provider = new TestAsyncQueryProvider<T>(this.AsQueryable().Provider);
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider
        {
            get { return _provider; }
        }
    }

    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;

        public TestAsyncEnumerator(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
        }

        public T Current => _enumerator.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_enumerator.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _enumerator.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}