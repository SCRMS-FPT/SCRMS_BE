using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using Xunit;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Coach.API.Features.Packages.CreatePackage;

namespace Coach.API.Tests.Packages
{
    public class CreatePackageEndpointTests
    {
        // Test 1: Tạo package thành công (Normal)
        [Fact]
        public async Task CreatePackage_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var request = new CreatePackageRequest(coachId, "Test", "Test", 100m, 5);
            var mockSender = new Mock<ISender>();
            mockSender.Setup(s => s.Send(It.IsAny<CreatePackageCommand>(), CancellationToken.None)).ReturnsAsync(new CreatePackageResult(Guid.NewGuid()));

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString())
            }));

            // Act
            var result = await CreatePackageEndpoint.HandleCreatePackage(request, mockSender.Object, httpContext);

            // Assert
            var createdResult = Assert.IsType<Created<CreatePackageResult>>(result);
            Assert.Equal($"/packages/{((CreatePackageResult)createdResult.Value).Id}", createdResult.Location);
        }

        // Test 2: Token không hợp lệ (Abnormal)
        [Fact]
        public async Task CreatePackage_InvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var request = new CreatePackageRequest(Guid.NewGuid(), "Test", "Test", 100m, 5);
            var mockSender = new Mock<ISender>();
            var httpContext = new DefaultHttpContext(); // Không có claims

            // Act
            var result = await CreatePackageEndpoint.HandleCreatePackage(request, mockSender.Object, httpContext);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
        }

        // Test 3: Token có claim không phải Guid (Abnormal)
        [Fact]
        public async Task CreatePackage_InvalidGuidClaim_ReturnsUnauthorized()
        {
            // Arrange
            var request = new CreatePackageRequest(Guid.NewGuid(), "Test", "Test", 100m, 5);
            var mockSender = new Mock<ISender>();
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "invalid-guid")
            }));

            // Act
            var result = await CreatePackageEndpoint.HandleCreatePackage(request, mockSender.Object, httpContext);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
        }

        // Test 4: Dữ liệu request không hợp lệ - Price âm (Abnormal)
        [Fact]
        public async Task CreatePackage_InvalidPrice_ShouldThrowValidationException()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var request = new CreatePackageRequest(coachId, "Test", "Test", -10m, 5);
            var mockSender = new Mock<ISender>();
            mockSender.Setup(s => s.Send(It.IsAny<CreatePackageCommand>(), CancellationToken.None)).ThrowsAsync(new ValidationException("Price must be greater than 0"));

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString())
            }));

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => CreatePackageEndpoint.HandleCreatePackage(request, mockSender.Object, httpContext));
        }

        // Test 5: Dữ liệu request không hợp lệ - SessionCount bằng 0 (Abnormal)
        [Fact]
        public async Task CreatePackage_InvalidSessionCount_ShouldThrowValidationException()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var request = new CreatePackageRequest(coachId, "Test", "Test", 100m, 0);
            var mockSender = new Mock<ISender>();
            mockSender.Setup(s => s.Send(It.IsAny<CreatePackageCommand>(), CancellationToken.None)).ThrowsAsync(new ValidationException("SessionCount must be greater than 0"));

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString())
            }));

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => CreatePackageEndpoint.HandleCreatePackage(request, mockSender.Object, httpContext));
        }

        // Test 6: CoachId trong request khác với coachId từ token (Boundary)
        [Fact]
        public async Task CreatePackage_DifferentCoachId_UsesTokenCoachId()
        {
            // Arrange
            var tokenCoachId = Guid.NewGuid();
            var requestCoachId = Guid.NewGuid();
            var request = new CreatePackageRequest(requestCoachId, "Test", "Test", 100m, 5);
            var mockSender = new Mock<ISender>();
            mockSender.Setup(s => s.Send(It.Is<CreatePackageCommand>(c => c.CoachId == tokenCoachId), CancellationToken.None)).ReturnsAsync(new CreatePackageResult(Guid.NewGuid()));

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, tokenCoachId.ToString())
            }));

            // Act
            var result = await CreatePackageEndpoint.HandleCreatePackage(request, mockSender.Object, httpContext);

            // Assert
            mockSender.Verify(s => s.Send(It.Is<CreatePackageCommand>(c => c.CoachId == tokenCoachId), CancellationToken.None), Times.Once);
            Assert.IsType<Created<CreatePackageResult>>(result);
        }

        // Test 7: Giá trị tối đa cho Price và SessionCount (Boundary)
        [Fact]
        public async Task CreatePackage_MaxValues_CreatesPackageSuccessfully()
        {
            // Arrange
            var coachId = Guid.NewGuid();
            var request = new CreatePackageRequest(coachId, "Test", "Test", decimal.MaxValue, int.MaxValue);
            var mockSender = new Mock<ISender>();
            mockSender.Setup(s => s.Send(It.IsAny<CreatePackageCommand>(), CancellationToken.None)).ReturnsAsync(new CreatePackageResult(Guid.NewGuid()));

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, coachId.ToString())
            }));

            // Act
            var result = await CreatePackageEndpoint.HandleCreatePackage(request, mockSender.Object, httpContext);

            // Assert
            Assert.IsType<Created<CreatePackageResult>>(result);
        }
    }
}