using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Moq;
using System.Linq;

namespace Coach.API.Tests
{
    public class TestEndpointRouteBuilder : IEndpointRouteBuilder
    {
        public ICollection<EndpointDataSource> DataSources { get; } = new List<EndpointDataSource>();
        public IServiceProvider ServiceProvider { get; }

        public class RouteInfo
        {
            public string Pattern { get; set; }
            public string Method { get; set; }
            public Delegate Handler { get; set; }

            // Add InvokeAsync method to RouteInfo
            public async Task<IResult> InvokeAsync(HttpContext httpContext, params object[] args)
            {
                // Prepare the arguments for dynamic invocation
                var parameters = new List<object> { httpContext };
                if (args != null && args.Length > 0)
                {
                    parameters.AddRange(args);
                }

                var result = Handler.DynamicInvoke(parameters.ToArray());

                if (result is Task<IResult> taskResult)
                {
                    return await taskResult;
                }
                else if (result is IResult directResult)
                {
                    return directResult;
                }
                else if (result is Task task)
                {
                    await task;
                    return Results.Ok();
                }

                return Results.Ok();
            }
        }

        public List<RouteInfo> Routes { get; } = new List<RouteInfo>();

        // Add parameterless constructor that creates a mock service provider
        public TestEndpointRouteBuilder()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            ServiceProvider = mockServiceProvider.Object;
        }

        public TestEndpointRouteBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IApplicationBuilder CreateApplicationBuilder()
        {
            return new Mock<IApplicationBuilder>().Object;
        }

        // Helper method to add a route
        public void AddRoute(string pattern, string method, Delegate handler)
        {
            Routes.Add(new RouteInfo { Pattern = pattern, Method = method, Handler = handler });
        }

        // Get a route safely
        public RouteInfo GetRoute(int index)
        {
            if (Routes.Count == 0)
            {
                throw new InvalidOperationException("No routes were added by the endpoint");
            }

            if (index < 0 || index >= Routes.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Route index is out of range");
            }

            return Routes[index];
        }

        // Get a route by pattern
        public RouteInfo GetRouteByPattern(string pattern)
        {
            var route = Routes.FirstOrDefault(r => r.Pattern == pattern);
            if (route == null)
            {
                throw new InvalidOperationException($"No route found with pattern '{pattern}'");
            }
            return route;
        }
    }

    public static class TestEndpointHelpers
    {
        public static TestEndpointRouteBuilder CreateTestEndpointBuilder()
        {
            var serviceProvider = new Mock<IServiceProvider>().Object;
            return new TestEndpointRouteBuilder(serviceProvider);
        }

        public static async Task<IResult> InvokeRouteHandler(TestEndpointRouteBuilder builder, int routeIndex, HttpContext httpContext)
        {
            var route = builder.GetRoute(routeIndex);

            if (route == null || route.Handler == null)
            {
                throw new InvalidOperationException("Route or handler is null");
            }

            try
            {
                if (route.Handler is Delegate handler)
                {
                    return await InvokeDelegateAsync(handler, httpContext);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error invoking route handler: {ex.Message}", ex);
            }

            throw new InvalidOperationException("Unable to invoke route handler");
        }

        private static async Task<IResult> InvokeDelegateAsync(Delegate handler, HttpContext httpContext)
        {
            var parameters = handler.Method.GetParameters();

            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(HttpContext))
            {
                var result = handler.DynamicInvoke(httpContext);

                if (result is Task<IResult> taskResult)
                {
                    return await taskResult;
                }
                else if (result is IResult directResult)
                {
                    return directResult;
                }
            }

            throw new InvalidOperationException("Handler has an unsupported signature");
        }
    }

    // Extension methods to simulate the endpoint route builder extensions
    public static class TestEndpointRouteBuilderExtensions
    {
        public static RouteHandlerBuilder MapGet(this TestEndpointRouteBuilder builder, string pattern, Delegate handler)
        {
            builder.AddRoute(pattern, "GET", handler);
            return new Mock<RouteHandlerBuilder>().Object;
        }

        public static RouteHandlerBuilder MapPost(this TestEndpointRouteBuilder builder, string pattern, Delegate handler)
        {
            builder.AddRoute(pattern, "POST", handler);
            return new Mock<RouteHandlerBuilder>().Object;
        }

        public static RouteHandlerBuilder MapPut(this TestEndpointRouteBuilder builder, string pattern, Delegate handler)
        {
            builder.AddRoute(pattern, "PUT", handler);
            return new Mock<RouteHandlerBuilder>().Object;
        }

        public static RouteHandlerBuilder MapDelete(this TestEndpointRouteBuilder builder, string pattern, Delegate handler)
        {
            builder.AddRoute(pattern, "DELETE", handler);
            return new Mock<RouteHandlerBuilder>().Object;
        }
    }
}