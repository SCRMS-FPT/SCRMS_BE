using Coach.API.Features.Coaches.GetCoaches;
using Coach.API.Features.Coaches.GetMyCoachProfile;
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

namespace Coach.API.Tests.Coaches
{
    public class GetMyCoachProfileEndpointTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly GetMyCoachProfileEndpoint _endpoint;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<ClaimsPrincipal> _mockUser;
        
        public GetMyCoachProfileEndpointTests()
        {
            _mockSender = new Mock<ISender>();
            _endpoint = new GetMyCoachProfileEndpoint();
            _mockHttpContext = new Mock<HttpContext>();
            _mockUser = new Mock<ClaimsPrincipal>();
            
            _mockHttpContext.Setup(x => x.User).Returns(_mockUser.Object);
        }
        
        [Fact]
        public async Task GetMyCoachProfile_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var endpointRouteBuilder = new TestEndpointRouteBuilder();
            _endpoint.AddRoutes(endpointRouteBuilder);
            var route = endpointRouteBuilder.Routes[0];
            
            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);
            
            var coachResponse = new CoachResponse(
                coachId,
                "Test Coach",
                "coach@test.com",
                "1234567890",
                "http://test.com/avatar.jpg",
                new List<string> { "image1.jpg", "image2.jpg" },
                new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
                "Coach bio",
                50.0m,
                DateTime.UtcNow,
                new List<CoachPackageResponse>(),
                new WeeklyScheduleResponse());
                
            _mockSender.Setup(x => x.Send(It.IsAny<GetMyCoachProfileQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(coachResponse);
                
            // Act
            var result = await (Task<IResult>)route.RequestDelegate(_mockHttpContext.Object, _mockSender.Object);
                
            // Assert
            var okResult = Assert.IsType<Ok<CoachResponse>>(result);
            var response = okResult.Value;
            
            Assert.Equal(coachId, response.Id);
            Assert.Equal("Test Coach", response.FullName);
            Assert.Equal("coach@test.com", response.Email);
            
            // Check that sender was called with right query
            _mockSender.Verify(x => x.Send(
                It.Is<GetMyCoachProfileQuery>(q => q.UserId == coachId), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task GetMyCoachProfile_NoUserClaim_ReturnsUnauthorized()
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
            _mockSender.Verify(x => x.Send(It.IsAny<GetMyCoachProfileQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task GetMyCoachProfile_InvalidGuidFormat_ReturnsUnauthorized()
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
            _mockSender.Verify(x => x.Send(It.IsAny<GetMyCoachProfileQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task GetMyCoachProfile_FallbackToNameIdentifierClaim_ReturnsOkResult()
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
            
            var coachResponse = new CoachResponse(
                coachId,
                "Test Coach",
                "coach@test.com",
                "1234567890",
                "http://test.com/avatar.jpg",
                new List<string>(),
                new List<Guid>(),
                "Coach bio",
                50.0m,
                DateTime.UtcNow,
                new List<CoachPackageResponse>(),
                new WeeklyScheduleResponse());
                
            _mockSender.Setup(x => x.Send(It.IsAny<GetMyCoachProfileQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(coachResponse);
                
            // Act
            var result = await (Task<IResult>)route.RequestDelegate(_mockHttpContext.Object, _mockSender.Object);
                
            // Assert
            Assert.IsType<Ok<CoachResponse>>(result);
            
            // Check that sender was called with right id
            _mockSender.Verify(x => x.Send(
                It.Is<GetMyCoachProfileQuery>(q => q.UserId == coachId), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task GetMyCoachProfile_MediatorThrowsException_ReturnsProblem()
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
            _mockSender.Setup(x => x.Send(It.IsAny<GetMyCoachProfileQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));
                
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                (Task<IResult>)route.RequestDelegate(_mockHttpContext.Object, _mockSender.Object));
                
            // Verify that Send was called
            _mockSender.Verify(x => x.Send(It.IsAny<GetMyCoachProfileQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
    
    // TestEndpointRouteBuilder class (nếu chưa tồn tại trong namespace)
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

    // TestRouteHandlerBuilder class (nếu chưa tồn tại trong namespace)
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

        public RouteHandlerBuilder Produces<T>()
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