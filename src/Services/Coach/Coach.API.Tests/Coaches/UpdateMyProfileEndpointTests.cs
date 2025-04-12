using Coach.API.Features.Coaches.UpdateCoach;
using Coach.API.Features.Coaches.UpdateMyProfile;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Coach.API.Tests.Coaches
{
    public class UpdateMyProfileEndpointTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly UpdateMyProfileEndpoint _endpoint;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<ClaimsPrincipal> _mockUser;
        private readonly MockFormFile _mockFile;
        
        public UpdateMyProfileEndpointTests()
        {
            _mockSender = new Mock<ISender>();
            _endpoint = new UpdateMyProfileEndpoint();
            _mockHttpContext = new Mock<HttpContext>();
            _mockUser = new Mock<ClaimsPrincipal>();
            _mockFile = new MockFormFile();
            
            _mockHttpContext.Setup(x => x.User).Returns(_mockUser.Object);
        }
        
        [Fact]
        public async Task UpdateMyProfile_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var endpointRouteBuilder = new TestEndpointRouteBuilder();
            _endpoint.AddRoutes(endpointRouteBuilder);
            var route = endpointRouteBuilder.Routes[0];
            
            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            var claims = new List<Claim> { claim };
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);
            
            var request = new UpdateCoachRequest
            {
                FullName = "Updated Coach",
                Email = "updated@coach.com",
                Phone = "0987654321",
                Bio = "Updated bio",
                RatePerHour = 75.0m,
                NewAvatar = _mockFile,
                NewImages = new List<IFormFile> { _mockFile },
                ExistingImageUrls = new List<string> { "image1.jpg" },
                ImagesToDelete = new List<string> { "old-image.jpg" },
                ListSport = new List<Guid> { Guid.NewGuid() }
            };
            
            _mockSender.Setup(x => x.Send(It.IsAny<UpdateCoachCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MediatR.Unit.Value);
                
            // Act
            var result = await (Task<IResult>)route.RequestDelegate(
                _mockHttpContext.Object, 
                request, 
                _mockSender.Object);
                
            // Assert
            var okResult = Assert.IsType<Ok<object>>(result);
            var response = okResult.Value as dynamic;
            
            // Check that sender was called with right command
            _mockSender.Verify(x => x.Send(
                It.Is<UpdateCoachCommand>(c => 
                    c.CoachId == coachId &&
                    c.FullName == request.FullName &&
                    c.Email == request.Email &&
                    c.Phone == request.Phone &&
                    c.Bio == request.Bio &&
                    c.RatePerHour == request.RatePerHour
                ), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task UpdateMyProfile_NoUserClaim_ReturnsUnauthorized()
        {
            // Arrange
            var endpointRouteBuilder = new TestEndpointRouteBuilder();
            _endpoint.AddRoutes(endpointRouteBuilder);
            var route = endpointRouteBuilder.Routes[0];
            
            // Set up HttpContext with no user claim
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns((Claim)null);
            _mockUser.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns((Claim)null);
            
            var request = new UpdateCoachRequest
            {
                FullName = "Updated Coach",
                Email = "updated@coach.com"
            };
            
            // Act
            var result = await (Task<IResult>)route.RequestDelegate(
                _mockHttpContext.Object, 
                request, 
                _mockSender.Object);
                
            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
            
            // Verify that Send was never called
            _mockSender.Verify(x => x.Send(It.IsAny<UpdateCoachCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task UpdateMyProfile_InvalidGuidFormat_ReturnsUnauthorized()
        {
            // Arrange
            var endpointRouteBuilder = new TestEndpointRouteBuilder();
            _endpoint.AddRoutes(endpointRouteBuilder);
            var route = endpointRouteBuilder.Routes[0];
            
            // Set up HttpContext with invalid GUID format
            var claim = new Claim(JwtRegisteredClaimNames.Sub, "not-a-guid");
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);
            
            var request = new UpdateCoachRequest
            {
                FullName = "Updated Coach",
                Email = "updated@coach.com"
            };
            
            // Act
            var result = await (Task<IResult>)route.RequestDelegate(
                _mockHttpContext.Object, 
                request, 
                _mockSender.Object);
                
            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
            
            // Verify that Send was never called
            _mockSender.Verify(x => x.Send(It.IsAny<UpdateCoachCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task UpdateMyProfile_FallbackToNameIdentifierClaim_ReturnsOkResult()
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
            
            var request = new UpdateCoachRequest
            {
                FullName = "Updated Coach",
                Email = "updated@coach.com",
                Phone = "0987654321",
                Bio = "Updated bio",
                RatePerHour = 75.0m
            };
            
            _mockSender.Setup(x => x.Send(It.IsAny<UpdateCoachCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MediatR.Unit.Value);
                
            // Act
            var result = await (Task<IResult>)route.RequestDelegate(
                _mockHttpContext.Object, 
                request, 
                _mockSender.Object);
                
            // Assert
            Assert.IsType<Ok<object>>(result);
            
            // Check that sender was called with right id
            _mockSender.Verify(x => x.Send(
                It.Is<UpdateCoachCommand>(c => c.CoachId == coachId), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task UpdateMyProfile_WithoutOptionalParameters_ReturnsOkResult()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var endpointRouteBuilder = new TestEndpointRouteBuilder();
            _endpoint.AddRoutes(endpointRouteBuilder);
            var route = endpointRouteBuilder.Routes[0];
            
            // Set up HttpContext with valid user claim
            var claim = new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString());
            _mockUser.Setup(u => u.FindFirst(JwtRegisteredClaimNames.Sub)).Returns(claim);
            
            // Request without optional parameters
            var request = new UpdateCoachRequest
            {
                FullName = "Updated Coach",
                Email = "updated@coach.com",
                Phone = "0987654321",
                Bio = "Updated bio",
                RatePerHour = 75.0m,
                // No avatar, no images, no sports
            };
            
            _mockSender.Setup(x => x.Send(It.IsAny<UpdateCoachCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MediatR.Unit.Value);
                
            // Act
            var result = await (Task<IResult>)route.RequestDelegate(
                _mockHttpContext.Object, 
                request, 
                _mockSender.Object);
                
            // Assert
            Assert.IsType<Ok<object>>(result);
            
            // Check that sender was called with empty lists for optional parameters
            _mockSender.Verify(x => x.Send(
                It.Is<UpdateCoachCommand>(c => 
                    c.NewAvatarFile == null && 
                    c.NewImageFiles.Count == 0 &&
                    c.ExistingImageUrls.Count == 0 &&
                    c.ImagesToDelete.Count == 0
                ), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
    
    // Mock classes to help with testing
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

        public RouteHandlerBuilder DisableAntiforgery()
        {
            return this;
        }

        public RouteHandlerBuilder RequireAuthorization(string policy)
        {
            return this;
        }

        public RouteHandlerBuilder WithName(string name)
        {
            return this;
        }

        public RouteHandlerBuilder Produces(int statusCode)
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

        public RouteHandlerBuilder WithSummary(string summary)
        {
            return this;
        }

        public RouteHandlerBuilder WithDescription(string description)
        {
            return this;
        }

        public RouteHandlerBuilder WithTags(string tag)
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