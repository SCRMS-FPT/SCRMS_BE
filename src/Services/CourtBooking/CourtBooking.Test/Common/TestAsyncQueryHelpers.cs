using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using System.Reflection;

namespace CourtBooking.Test.Common
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> AsAsyncQueryable<T>(this IQueryable<T> queryable)
        {
            return new TestAsyncQueryable<T>(queryable);
        }
        
        public static IQueryable<T> AsAsyncQueryable<T>(this T[] array)
        {
            return new TestAsyncQueryable<T>(array.AsQueryable());
        }
        
        public static IQueryable<T> AsAsyncQueryable<T>(this List<T> list)
        {
            return new TestAsyncQueryable<T>(list.AsQueryable());
        }
    }

    internal class TestAsyncQueryable<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>
    {
        private readonly IQueryable<T> _inner;

        internal TestAsyncQueryable(IQueryable<T> inner)
        {
            _inner = inner;
        }

        public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _inner.GetEnumerator();
        public Type ElementType => _inner.ElementType;
        public Expression Expression => _inner.Expression;
        
        public IQueryProvider Provider => new TestAsyncQueryProvider<T>(_inner.Provider);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(_inner.GetEnumerator());
        }

        public IOrderedQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            var orderedQueryable = Queryable.OrderBy(_inner, keySelector);
            return new TestAsyncQueryable<T>(orderedQueryable);
        }

        public IOrderedQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            var orderedQueryable = Queryable.OrderByDescending(_inner, keySelector);
            return new TestAsyncQueryable<T>(orderedQueryable);
        }

        public IOrderedQueryable<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (_inner is IOrderedQueryable<T> orderedQueryable)
            {
                var thenByQueryable = Queryable.ThenBy(orderedQueryable, keySelector);
                return new TestAsyncQueryable<T>(thenByQueryable);
            }
            
            // Nếu không phải OrderedQueryable, xử lý như OrderBy thông thường
            return OrderBy(keySelector);
        }

        public IOrderedQueryable<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (_inner is IOrderedQueryable<T> orderedQueryable)
            {
                var thenByDescendingQueryable = Queryable.ThenByDescending(orderedQueryable, keySelector);
                return new TestAsyncQueryable<T>(thenByDescendingQueryable);
            }
            
            // Nếu không phải OrderedQueryable, xử lý như OrderByDescending thông thường
            return OrderByDescending(keySelector);
        }
    }

    internal class TestAsyncQueryProvider<T> : IQueryProvider, IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            // Phát hiện kiểu generic từ expression
            Type elementType = expression.Type.GetGenericArguments().First();
            
            // Tạo IQueryable generic với ElementType đúng
            Type queryableType = typeof(TestAsyncQueryable<>).MakeGenericType(elementType);
            
            // Tạo IQueryable từ inner provider
            var innerQuery = _inner.CreateQuery(expression);
            
            // Tạo thể hiện của TestAsyncQueryable<T> với tham số là innerQuery
            return (IQueryable)Activator.CreateInstance(queryableType, innerQuery);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncQueryable<TElement>(_inner.CreateQuery<TElement>(expression));
        }

        public object Execute(Expression expression) => _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            // Đảm bảo đây là một task
            Type taskType = typeof(TResult);
            if (!taskType.IsGenericType || !taskType.GetGenericTypeDefinition().IsAssignableFrom(typeof(Task<>)))
            {
                // Không phải task, thực thi bình thường
                return Execute<TResult>(expression);
            }
            
            // Lấy loại generic của Task<T>
            Type taskInnerType = taskType.GetGenericArguments()[0];
            
            // Xử lý đặc biệt cho LongCountAsync, CountAsync, và các phương thức tập hợp
            if (expression is MethodCallExpression methodCallExpr)
            {
                var methodName = methodCallExpr.Method.Name;
                
                // Xử lý LongCountAsync và CountAsync
                if (methodName == "LongCountAsync" || methodName == "CountAsync" || 
                    methodName.StartsWith("Count") || methodName.StartsWith("LongCount"))
                {
                    // Lấy nguồn dữ liệu từ biểu thức và thực hiện Count trực tiếp
                    IQueryable source = null;
                    if (methodCallExpr.Arguments.Count > 0 && 
                        methodCallExpr.Arguments[0] is ConstantExpression constExpr &&
                        constExpr.Value is IQueryable queryable)
                    {
                        source = queryable;
                    }
                    else if (_inner is IQueryProvider provider && 
                             provider.GetType().Name.Contains("EnumerableQuery"))
                    {
                        // Lấy dữ liệu từ EnumerableQuery
                        var enumerableQuery = provider.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                            .FirstOrDefault(f => f.Name == "enumerable" || f.Name == "_enumerable" || f.Name == "_source");
                        
                        if (enumerableQuery != null)
                        {
                            var enumerable = enumerableQuery.GetValue(provider) as System.Collections.IEnumerable;
                            if (enumerable != null)
                            {
                                var count = enumerable.Cast<object>().Count();
                                var result = taskInnerType == typeof(long) ? (object)(long)count : (object)count;
                                
                                var fromResultMethod = typeof(Task).GetMethod(
                                    nameof(Task.FromResult),
                                    BindingFlags.Public | BindingFlags.Static)
                                    .MakeGenericMethod(taskInnerType);
                                
                                return (TResult)fromResultMethod.Invoke(null, new object[] { result });
                            }
                        }
                    }
                    
                    // Nếu có thể, đếm trực tiếp từ nguồn
                    if (source != null)
                    {
                        int count = source.Cast<object>().Count();
                        var result = taskInnerType == typeof(long) ? (object)(long)count : (object)count;
                        
                        var fromResultMethod = typeof(Task).GetMethod(
                            nameof(Task.FromResult),
                            BindingFlags.Public | BindingFlags.Static)
                            .MakeGenericMethod(taskInnerType);
                        
                        return (TResult)fromResultMethod.Invoke(null, new object[] { result });
                    }
                }
                
                // Xử lý ToListAsync, FirstOrDefaultAsync, SingleOrDefaultAsync, v.v.
                if (methodName.Contains("Async") || methodName.Contains("List") || 
                    methodName.Contains("First") || methodName.Contains("Single"))
                {
                    // Thử lấy nguồn dữ liệu
                    IQueryable source = null;
                    if (methodCallExpr.Arguments.Count > 0)
                    {
                        var arg = methodCallExpr.Arguments[0];
                        if (arg is ConstantExpression constExpr && constExpr.Value is IQueryable queryable)
                        {
                            source = queryable;
                        }
                    }
                    
                    // Thực hiện phương thức cụ thể trên nguồn
                    if (source != null)
                    {
                        var result = methodName switch
                        {
                            var m when m.Contains("First") => source.Cast<object>().FirstOrDefault(),
                            var m when m.Contains("Single") => source.Cast<object>().SingleOrDefault(),
                            var m when m.Contains("Last") => source.Cast<object>().LastOrDefault(),
                            var m when m.Contains("List") => source.Cast<object>().ToList(),
                            _ => null
                        };
                        
                        var fromResultMethod = typeof(Task).GetMethod(
                            nameof(Task.FromResult),
                            BindingFlags.Public | BindingFlags.Static)
                            .MakeGenericMethod(taskInnerType);
                        
                        return (TResult)fromResultMethod.Invoke(null, new object[] { result });
                    }
                }
            }
            
            // Phương pháp dự phòng: tạo kết quả mặc định
            try
            {
                var defaultValue = taskInnerType.IsValueType ? Activator.CreateInstance(taskInnerType) : null;
                var taskFromResultMethod = typeof(Task).GetMethod(
                    nameof(Task.FromResult),
                    BindingFlags.Public | BindingFlags.Static)
                    .MakeGenericMethod(taskInnerType);
                
                return (TResult)taskFromResultMethod.Invoke(null, new object[] { defaultValue });
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine($"Lỗi trong ExecuteAsync: {ex.Message}");
                
                // Trả về giá trị Task mặc định
                return default;
            }
        }

        private object Visit(Expression expression)
        {
            if (expression is ConstantExpression constExpr)
            {
                return constExpr.Value;
            }
            return _inner.Execute(expression);
        }

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return new TestAsyncEnumerable<TResult>(_inner.CreateQuery<TResult>(expression));
        }
    }

    internal class TestAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly IEnumerable<T> _enumerable;

        public TestAsyncEnumerable(IEnumerable<T> enumerable)
        {
            _enumerable = enumerable;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(_enumerable.GetEnumerator());
        }
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
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

    public static class AsyncQueryableExtensions
    {
        public static IQueryable<T> AsAsyncQueryable<T>(this IEnumerable<T> source)
        {
            return new TestAsyncQueryable<T>(source.AsQueryable());
        }

        public static IOrderedQueryable<T> OrderBy<T, TKey>(this IQueryable<T> source, Expression<Func<T, TKey>> keySelector)
        {
            if (source is TestAsyncQueryable<T> asyncQueryable)
            {
                return asyncQueryable.OrderBy(keySelector);
            }
            return Queryable.OrderBy(source, keySelector);
        }

        public static IOrderedQueryable<T> OrderByDescending<T, TKey>(this IQueryable<T> source, Expression<Func<T, TKey>> keySelector)
        {
            if (source is TestAsyncQueryable<T> asyncQueryable)
            {
                return asyncQueryable.OrderByDescending(keySelector);
            }
            return Queryable.OrderByDescending(source, keySelector);
        }
    }
} 