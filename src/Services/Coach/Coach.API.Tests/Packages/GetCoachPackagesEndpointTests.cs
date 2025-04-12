using Coach.API.Features.Packages.GetActivePackages;
using Coach.API.Features.Packages.GetCoachPackages;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Coach.API.Tests.Packages
{
    public class GetCoachPackagesEndpointTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly GetCoachPackagesEndpoint _endpoint;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<ClaimsPrincipal> _mockUser;
        
        public GetCoachPackagesEndpointTests()
        {
            _mockSender = new Mock<ISender>();
            _endpoint = new GetCoachPackagesEndpoint();
            _mockHttpContext = new Mock<HttpContext>();
            _mockUser = new Mock<ClaimsPrincipal>();
            
            _mockHttpContext.Setup(x => x.User).Returns(_mockUser.Object);
        }
        
        [Fact]
        public async Task GetCoachPackages_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var endpointRouteBuilder = new TestEndpointRouteBuilder();
            _endpoint.AddRoutes(endpointRouteBuilder);
            var route = endpointRouteBuilder.Routes[0];
            
            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);
            
            var packages = new List<PackageResponse>
            {
                new(Guid.NewGuid(), coachId, "Package 1", "Description 1", 100.0m, 5, 1, DateTime.UtcNow, DateTime.UtcNow),
                new(Guid.NewGuid(), coachId, "Package 2", "Description 2", 200.0m, 10, 1, DateTime.UtcNow, DateTime.UtcNow)
            };
                
            _mockSender.Setup(x => x.Send(It.IsAny<GetCoachPackagesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(packages);
                
            // Act
            var result = await (Task<IResult>)route.RequestDelegate(_mockHttpContext.Object, _mockSender.Object);
                
            // Assert
            var okResult = Assert.IsType<Ok<List<PackageResponse>>>(result);
            var response = okResult.Value;
            
            Assert.NotNull(response);
            Assert.Equal(2, response.Count);
            
            // Check that sender was called with right query
            _mockSender.Verify(x => x.Send(
                It.Is<GetCoachPackagesQuery>(q => q.CoachId == coachId), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task GetCoachPackages_NoUserClaim_ReturnsUnauthorized()
        {
            // Arrange
            var endpointRouteBuilder = new TestEndpointRouteBuilder();
            _endpoint.AddRoutes(endpointRouteBuilder);
            var route = endpointRouteBuilder.Routes[0];
            
            // Set up HttpContext with no user claim
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns((Claim)null);
            _mockUser.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns((Claim)null);
                
            // Act
            var result = await (Task<IResult>)route.RequestDelegate(_mockHttpContext.Object, _mockSender.Object);
                
            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
            
            // Verify that Send was never called
            _mockSender.Verify(x => x.Send(It.IsAny<GetCoachPackagesQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task GetCoachPackages_InvalidGuidFormat_ReturnsUnauthorized()
        {
            // Arrange
            var endpointRouteBuilder = new TestEndpointRouteBuilder();
            _endpoint.AddRoutes(endpointRouteBuilder);
            var route = endpointRouteBuilder.Routes[0];
            
            // Set up HttpContext with invalid GUID format
            var claim = new Claim(JwtRegisteredClaimNames.Sub, "not-a-guid");
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);
                
            // Act
            var result = await (Task<IResult>)route.RequestDelegate(_mockHttpContext.Object, _mockSender.Object);
                
            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
            
            // Verify that Send was never called
            _mockSender.Verify(x => x.Send(It.IsAny<GetCoachPackagesQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task GetCoachPackages_FallbackToNameIdentifierClaim_ReturnsOkResult()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var endpointRouteBuilder = new TestEndpointRouteBuilder();
            _endpoint.AddRoutes(endpointRouteBuilder);
            var route = endpointRouteBuilder.Routes[0];
            
            // Set up HttpContext with NameIdentifier claim as fallback
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns((Claim)null);
            var claim = new Claim(ClaimTypes.NameIdentifier, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns(claim);
            
            var packages = new List<PackageResponse>
            {
                new(Guid.NewGuid(), coachId, "Package 1", "Description 1", 100.0m, 5, 1, DateTime.UtcNow, DateTime.UtcNow)
            };
                
            _mockSender.Setup(x => x.Send(It.IsAny<GetCoachPackagesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(packages);
                
            // Act
            var result = await (Task<IResult>)route.RequestDelegate(_mockHttpContext.Object, _mockSender.Object);
                
            // Assert
            Assert.IsType<Ok<List<PackageResponse>>>(result);
            
            // Check that sender was called with right id
            _mockSender.Verify(x => x.Send(
                It.Is<GetCoachPackagesQuery>(q => q.CoachId == coachId), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task GetCoachPackages_EmptyPackagesList_ReturnsEmptyList()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var endpointRouteBuilder = new TestEndpointRouteBuilder();
            _endpoint.AddRoutes(endpointRouteBuilder);
            var route = endpointRouteBuilder.Routes[0];
            
            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);
            
            // Empty packages list
            var packages = new List<PackageResponse>();
                
            _mockSender.Setup(x => x.Send(It.IsAny<GetCoachPackagesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(packages);
                
            // Act
            var result = await (Task<IResult>)route.RequestDelegate(_mockHttpContext.Object, _mockSender.Object);
                
            // Assert
            var okResult = Assert.IsType<Ok<List<PackageResponse>>>(result);
            var response = okResult.Value;
            
            Assert.NotNull(response);
            Assert.Empty(response);
        }
        
        [Fact]
        public async Task GetCoachPackages_MediatorThrowsException_PropagatesException()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var endpointRouteBuilder = new TestEndpointRouteBuilder();
            _endpoint.AddRoutes(endpointRouteBuilder);
            var route = endpointRouteBuilder.Routes[0];
            
            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);
            
            // Set up mediator to throw exception
            _mockSender.Setup(x => x.Send(It.IsAny<GetCoachPackagesQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));
                
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                (Task<IResult>)route.RequestDelegate(_mockHttpContext.Object, _mockSender.Object));
                
            // Verify that Send was called
            _mockSender.Verify(x => x.Send(It.IsAny<GetCoachPackagesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
    
    // Test helper classes - nếu chưa có trong namespace, thêm chúng vào
    public class TestEndpointRouteBuilder : IEndpointRouteBuilder
    {
        public ICollection<EndpointDataSource> DataSources => throw new NotImplementedException();

        public IServiceProvider ServiceProvider => throw new NotImplementedException();

        public List<RouteHandlerBuilder> Routes { get; } = new List<RouteHandlerBuilder>();

        public IApplicationBuilder CreateApplicationBuilder() => throw new NotImplementedException();

        public RouteHandlerBuilder MapGet(string pattern, Delegate handler)
        {
            var routeBuilder = new TestRouteHandlerBuilder(handler);
            Routes.Add(routeBuilder);
            return routeBuilder;
        }

        public RouteHandlerBuilder MapPost(string pattern, Delegate handler)
        {
            var routeBuilder = new TestRouteHandlerBuilder(handler);
            Routes.Add(routeBuilder);
            return routeBuilder;
        }

        public RouteHandlerBuilder MapPut(string pattern, Delegate handler)
        {
            var routeBuilder = new TestRouteHandlerBuilder(handler);
            Routes.Add(routeBuilder);
            return routeBuilder;
        }

        public RouteHandlerBuilder MapDelete(string pattern, Delegate handler)
        {
            var routeBuilder = new TestRouteHandlerBuilder(handler);
            Routes.Add(routeBuilder);
            return routeBuilder;
        }

        public RouteHandlerBuilder MapMethods(string pattern, IEnumerable<string> httpMethods, Delegate handler)
        {
            var routeBuilder = new TestRouteHandlerBuilder(handler);
            Routes.Add(routeBuilder);
            return routeBuilder;
        }

        public IEndpointConventionBuilder Map(
            string pattern,
            Func<HttpContext, Task> requestDelegate)
        {
            throw new NotImplementedException();
        }

        public IEndpointConventionBuilder MapDelete(
            string pattern,
            RequestDelegate requestDelegate)
        {
            throw new NotImplementedException();
        }

        public IEndpointConventionBuilder MapGet(
            string pattern,
            RequestDelegate requestDelegate)
        {
            throw new NotImplementedException();
        }

        public IEndpointConventionBuilder MapMethods(
            string pattern,
            IEnumerable<string> httpMethods,
            RequestDelegate requestDelegate)
        {
            throw new NotImplementedException();
        }

        public IEndpointConventionBuilder MapPost(
            string pattern,
            RequestDelegate requestDelegate)
        {
            throw new NotImplementedException();
        }

        public IEndpointConventionBuilder MapPut(
            string pattern,
            RequestDelegate requestDelegate)
        {
            throw new NotImplementedException();
        }
    }

    public class TestRouteHandlerBuilder : RouteHandlerBuilder
    {
        public Delegate RequestDelegate { get; }

        public TestRouteHandlerBuilder(Delegate requestDelegate)
        {
            RequestDelegate = requestDelegate;
        }

        public override RouteHandlerBuilder Add(Func<RouteHandlerContext, RouteHandlerFilterDelegate, RouteHandlerFilterDelegate> handler)
        {
            return this;
        }

        public override RouteHandlerBuilder Finally<TFilterType>()
        {
            return this;
        }

        public override RouteHandlerBuilder Finally<TFilterType, TFilterOptions>(TFilterOptions options)
        {
            return this;
        }

        public override IReadOnlyList<object> GetMetadata()
        {
            throw new NotImplementedException();
        }

        public RouteHandlerBuilder RequireAuthorization(string policy)
        {
            return this;
        }

        public RouteHandlerBuilder WithName(string name)
        {
            return this;
        }

        public RouteHandlerBuilder Produces<T>(int statusCode)
        {
            return this;
        }

        public RouteHandlerBuilder ProducesProblem(int statusCode)
        {
            return this;
        }

        public RouteHandlerBuilder WithTags(string tag)
        {
            return this;
        }

        public RouteHandlerBuilder WithSummary(string summary)
        {
            return this;
        }

        public RouteHandlerBuilder WithDescription(string description)
        {
            return this;
        }
    }
} 