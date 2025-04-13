using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coach.API.Tests.TestHelpers
{
    public class TestEndpointRouteBuilder : IEndpointRouteBuilder
    {
        public IServiceProvider ServiceProvider => null;
        public ICollection<EndpointDataSource> DataSources => new List<EndpointDataSource>();

        public List<TestRouteHandlerBuilder> Routes { get; } = new List<TestRouteHandlerBuilder>();

        public IApplicationBuilder CreateApplicationBuilder() => throw new NotImplementedException();

        public TestRouteHandlerBuilder MapGet(string pattern, Delegate handler)
        {
            var routeBuilder = new TestRouteHandlerBuilder(handler);
            Routes.Add(routeBuilder);
            return routeBuilder;
        }

        public TestRouteHandlerBuilder MapPost(string pattern, Delegate handler)
        {
            var routeBuilder = new TestRouteHandlerBuilder(handler);
            Routes.Add(routeBuilder);
            return routeBuilder;
        }

        public TestRouteHandlerBuilder MapPut(string pattern, Delegate handler)
        {
            var routeBuilder = new TestRouteHandlerBuilder(handler);
            Routes.Add(routeBuilder);
            return routeBuilder;
        }

        public TestRouteHandlerBuilder MapDelete(string pattern, Delegate handler)
        {
            var routeBuilder = new TestRouteHandlerBuilder(handler);
            Routes.Add(routeBuilder);
            return routeBuilder;
        }

        public TestRouteHandlerBuilder MapMethods(string pattern, IEnumerable<string> httpMethods, Delegate handler)
        {
            var routeBuilder = new TestRouteHandlerBuilder(handler);
            Routes.Add(routeBuilder);
            return routeBuilder;
        }
    }

    public class TestRouteHandlerBuilder
    {
        public Delegate RequestDelegate { get; }

        public TestRouteHandlerBuilder(Delegate requestDelegate)
        {
            RequestDelegate = requestDelegate;
        }

        public async Task<object> InvokeAsync(params object[] args)
        {
            // Use reflection to invoke the delegate with the provided arguments
            return await Task.FromResult(RequestDelegate.DynamicInvoke(args));
        }

        public TestRouteHandlerBuilder Add(object handler)
        {
            return this;
        }

        public TestRouteHandlerBuilder DisableAntiforgery()
        {
            return this;
        }

        public TestRouteHandlerBuilder RequireAuthorization(string policy)
        {
            return this;
        }

        public TestRouteHandlerBuilder WithName(string name)
        {
            return this;
        }

        public TestRouteHandlerBuilder Produces(int statusCode)
        {
            return this;
        }

        public TestRouteHandlerBuilder Produces<T>(int statusCode)
        {
            return this;
        }

        public TestRouteHandlerBuilder Produces<T>()
        {
            return this;
        }

        public TestRouteHandlerBuilder ProducesProblem(int statusCode)
        {
            return this;
        }

        public TestRouteHandlerBuilder WithTags(string tag)
        {
            return this;
        }

        public TestRouteHandlerBuilder WithSummary(string summary)
        {
            return this;
        }

        public TestRouteHandlerBuilder WithDescription(string description)
        {
            return this;
        }
    }

    public class MockFormFile : IFormFile
    {
        public string ContentType => "image/jpeg";
        public string ContentDisposition => "form-data; name=\"file\"; filename=\"test.jpg\"";
        public IHeaderDictionary Headers => new HeaderDictionary();
        public long Length => 1024;
        public string Name => "file";
        public string FileName => "test.jpg";

        public void CopyTo(Stream target)
        {
            // Do nothing in mock
        }

        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Stream OpenReadStream()
        {
            return new MemoryStream();
        }
    }
}