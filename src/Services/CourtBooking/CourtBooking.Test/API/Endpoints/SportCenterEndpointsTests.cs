using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using CourtBooking.API.Endpoints;
using CourtBooking.Application.CourtManagement.Command.UpdateSportCenter;
using CourtBooking.Application.CourtManagement.Queries.GetAllCourtsOfSportCenter;
using CourtBooking.Application.CourtManagement.Queries.GetSportCenterById;
using CourtBooking.Application.CourtManagement.Queries.GetSportCenters;
using CourtBooking.Application.DTOs;
using CourtBooking.Application.SportCenterManagement.Commands.CreateSportCenter;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace CourtBooking.Test.API.Endpoints
{
    public class SportCenterEndpointsTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly SportCenterEndpoints _endpoints;
        private readonly Mock<HttpContext> _mockHttpContext;

        public SportCenterEndpointsTests()
        {
            _mockSender = new Mock<ISender>();
            _endpoints = new SportCenterEndpoints();
            _mockHttpContext = new Mock<HttpContext>();
        }

        #region GetSportCenters Tests

        [Fact]
        public async Task GetSportCenters_Should_ReturnOk_When_Called()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var sportCenterId = Guid.NewGuid();
            var sportCenters = new List<SportCenterListDTO>
            {
                new SportCenterListDTO(
                    sportCenterId,                // Id
                    "Sport Center 1",             // Name
                    "0987654321",                 // PhoneNumber
                    new List<string> { "Tennis", "Football" }, // SportNames
                    "123 Street, District 1, City", // Address
                    "A great sport center",       // Description
                    "avatar.jpg",                 // Avatar
                    new List<string> { "image1.jpg" }, // ImageUrl
                    new List<CourtListDTO>()      // Courts
                )
            };

            var paginatedResult = new PaginatedResult<SportCenterListDTO>(
                0,                   // pageIndex
                10,                  // pageSize
                sportCenters.Count,  // count
                sportCenters         // data
            );

            _mockSender.Setup(x => x.Send(It.IsAny<GetSportCentersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetSportCentersResult(paginatedResult));

            // Act
            var result = await InvokeGetSportCentersDelegate(1, 10, null, null);

            // Assert
            Assert.IsType<Ok<GetSportCentersResponse>>(result);
            var okResult = (Ok<GetSportCentersResponse>)result;
            Assert.Equal(sportCenters.Count, okResult.Value.SportCenters.Data.Count());
            Assert.Equal("Sport Center 1", okResult.Value.SportCenters.Data.First().Name);

            _mockSender.Verify(x => x.Send(It.IsAny<GetSportCentersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(0, 10)] // Invalid page
        [InlineData(1, 0)]  // Invalid page size
        [InlineData(1, 101)] // Page size exceeds max
        public async Task GetSportCenters_Should_HandleInvalidPagination(int page, int limit)
        {
            // Arrange
            _mockSender.Setup(x => x.Send(It.IsAny<GetSportCentersQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Invalid pagination parameters"));

            // Act
            var result = await InvokeGetSportCentersDelegate(page, limit, null, null);

            // Assert
            Assert.IsType<BadRequest<ProblemDetails>>(result);
        }

        [Fact]
        public async Task GetSportCenters_Should_FilterByCity()
        {
            // Arrange
            var city = "Hanoi";
            var sportCenterId = Guid.NewGuid();
            var sportCenters = new List<SportCenterListDTO>
            {
                new SportCenterListDTO(
                    sportCenterId,                // Id
                    "Sport Center Hanoi",         // Name
                    "0987654321",                 // PhoneNumber
                    new List<string> { "Tennis" }, // SportNames
                    $"123 Street, District 1, {city}", // Address includes city
                    "A sport center in Hanoi",    // Description
                    "avatar.jpg",                 // Avatar
                    new List<string> { "image1.jpg" }, // ImageUrl
                    new List<CourtListDTO>()      // Courts
                )
            };

            var paginatedResult = new PaginatedResult<SportCenterListDTO>(
                0,                   // pageIndex
                10,                  // pageSize
                sportCenters.Count,  // count
                sportCenters         // data
            );

            _mockSender.Setup(x => x.Send(
                It.Is<GetSportCentersQuery>(q => q.City == city),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetSportCentersResult(paginatedResult));

            // Act
            var result = await InvokeGetSportCentersDelegate(1, 10, city, null);

            // Assert
            Assert.IsType<Ok<GetSportCentersResponse>>(result);
            var okResult = (Ok<GetSportCentersResponse>)result;
            Assert.Contains(city, okResult.Value.SportCenters.Data.First().Address);
        }

        [Fact]
        public async Task GetSportCenters_Should_FilterByName()
        {
            // Arrange
            var name = "Center";
            var sportCenterId = Guid.NewGuid();
            var sportCenters = new List<SportCenterListDTO>
            {
                new SportCenterListDTO(
                    sportCenterId,                // Id
                    $"Sport {name}",              // Name includes the search term
                    "0987654321",                 // PhoneNumber
                    new List<string> { "Tennis" }, // SportNames
                    "123 Street, District 1, City", // Address
                    "A great sport center",       // Description
                    "avatar.jpg",                 // Avatar
                    new List<string> { "image1.jpg" }, // ImageUrl
                    new List<CourtListDTO>()      // Courts
                )
            };

            var paginatedResult = new PaginatedResult<SportCenterListDTO>(
                0,                   // pageIndex
                10,                  // pageSize
                sportCenters.Count,  // count
                sportCenters         // data
            );

            _mockSender.Setup(x => x.Send(
                It.Is<GetSportCentersQuery>(q => q.Name == name),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetSportCentersResult(paginatedResult));

            // Act
            var result = await InvokeGetSportCentersDelegate(1, 10, null, name);

            // Assert
            Assert.IsType<Ok<GetSportCentersResponse>>(result);
            var okResult = (Ok<GetSportCentersResponse>)result;
            Assert.Contains(name, okResult.Value.SportCenters.Data.First().Name);
        }

        #endregion GetSportCenters Tests

        #region GetSportCenterById Tests

        [Fact]
        public async Task GetSportCenterById_Should_ReturnOk_When_Exists()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            var sportCenterDetail = new SportCenterDetailDTO(
                sportCenterId,                // Id
                Guid.NewGuid(),               // OwnerId
                "Sport Center 1",             // Name
                "0987654321",                 // PhoneNumber
                "123 Street",                 // AddressLine
                "Hanoi",                      // City
                "District 1",                 // District
                "Ward 1",                     // Commune
                21.02,                        // Latitude
                105.83,                       // Longitude
                "avatar.jpg",                 // Avatar
                new List<string> { "image1.jpg" }, // ImageUrls
                false,                         // IsDeleted
                "Description",                // Description
                DateTime.UtcNow,              // CreatedAt
                null                          // LastModified
            );

            _mockSender.Setup(x => x.Send(
                It.Is<GetSportCenterByIdQuery>(q => q.Id == sportCenterId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetSportCenterByIdResult(sportCenterDetail));

            SetupAuthorizedUser("User");

            // Act
            var result = await InvokeGetSportCenterByIdDelegate(sportCenterId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Ok<SportCenterDetailDTO>>(result);
            var okResult = (Ok<SportCenterDetailDTO>)result;
            Assert.Equal(sportCenterId, okResult.Value.Id);
            Assert.Equal("Sport Center 1", okResult.Value.Name);
        }

        [Fact]
        public async Task GetSportCenterById_Should_ReturnNotFound_When_NotExists()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();

            _mockSender.Setup(x => x.Send(
                It.Is<GetSportCenterByIdQuery>(q => q.Id == sportCenterId),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException($"Không tìm thấy trung tâm thể thao với ID {sportCenterId}"));

            SetupAuthorizedUser("User");

            // Act
            var result = await InvokeGetSportCenterByIdDelegate(sportCenterId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<NotFound>(result);
        }

        [Fact]
        public async Task GetSportCenterById_Should_ReturnUnauthorized_When_NotAuthenticated()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            SetupUnauthenticatedUser();

            // Act
            var result = await InvokeGetSportCenterByIdDelegate(sportCenterId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
            _mockSender.Verify(x => x.Send(It.IsAny<GetSportCenterByIdQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion GetSportCenterById Tests

        #region CreateSportCenter Tests

        [Fact]
        public async Task CreateSportCenter_Should_ReturnCreated_When_Authenticated()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            SetupAuthorizedUser("SportCenterOwner", userId);

            var createCommand = new CreateSportCenterCommand(
                "Sport Center 1",               // Name
                "0987654321",                   // PhoneNumber
                "Đường 123",                    // AddressLine
                "Hà Nội",                       // City
                "Quận 1",                       // District
                "Phường 1",                     // Commune
                21.02,                          // Latitude
                105.83,                         // Longitude
                "avatar.jpg",                   // Avatar
                new List<string> { "image1.jpg", "image2.jpg" }, // ImageUrls
                "Mô tả trung tâm thể thao"      // Description
            );

            _mockSender.Setup(x => x.Send(It.IsAny<CreateSportCenterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateSportCenterResult(sportCenterId));

            // Act
            var result = await InvokeCreateSportCenterDelegate(createCommand, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Created<CreateSportCenterResponse>>(result);
            var created = (Created<CreateSportCenterResponse>)result;
            Assert.Equal($"/api/sportcenters/{sportCenterId}", created.Location);
            Assert.Equal(sportCenterId, created.Value.Id);

            _mockSender.Verify(x => x.Send(It.IsAny<CreateSportCenterCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateSportCenter_Should_ReturnBadRequest_When_InvalidData()
        {
            // Arrange
            SetupAuthorizedUser("SportCenterOwner");

            var invalidCommand = new CreateSportCenterCommand(
                "",                             // Name - trống
                "invalid-phone",                // PhoneNumber - không hợp lệ
                "Đường 123",                    // AddressLine
                "Hà Nội",                       // City
                "Quận 1",                       // District
                "Phường 1",                     // Commune
                21.02,                          // Latitude
                105.83,                         // Longitude
                "avatar.jpg",                   // Avatar
                new List<string>(),             // ImageUrls - trống
                "Mô tả"                         // Description
            );

            _mockSender.Setup(x => x.Send(It.IsAny<CreateSportCenterCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Dữ liệu không hợp lệ"));

            // Act
            var result = await InvokeCreateSportCenterDelegate(invalidCommand, _mockHttpContext.Object);

            // Assert
            Assert.IsType<BadRequest<ProblemDetails>>(result);
        }

        [Fact]
        public async Task CreateSportCenter_Should_ReturnUnauthorized_When_NotAuthenticated()
        {
            // Arrange
            SetupUnauthenticatedUser();

            var command = new CreateSportCenterCommand(
                "Sport Center 1",               // Name
                "0987654321",                   // PhoneNumber
                "Đường 123",                    // AddressLine
                "Hà Nội",                       // City
                "Quận 1",                       // District
                "Phường 1",                     // Commune
                21.02,                          // Latitude
                105.83,                         // Longitude
                "avatar.jpg",                   // Avatar
                new List<string> { "image1.jpg" }, // ImageUrls
                "Mô tả"                         // Description
            );

            // Act
            var result = await InvokeCreateSportCenterDelegate(command, _mockHttpContext.Object);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
            _mockSender.Verify(x => x.Send(It.IsAny<CreateSportCenterCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateSportCenter_Should_ReturnForbidden_When_RegularUser()
        {
            // Arrange
            SetupAuthorizedUser("User"); // Vai trò không đủ quyền

            var command = new CreateSportCenterCommand(
                "Sport Center 1",               // Name
                "0987654321",                   // PhoneNumber
                "Đường 123",                    // AddressLine
                "Hà Nội",                       // City
                "Quận 1",                       // District
                "Phường 1",                     // Commune
                21.02,                          // Latitude
                105.83,                         // Longitude
                "avatar.jpg",                   // Avatar
                new List<string> { "image1.jpg" }, // ImageUrls
                "Mô tả"                         // Description
            );

            // Act
            var result = await InvokeCreateSportCenterDelegate(command, _mockHttpContext.Object);

            // Assert
            Assert.IsType<ForbidHttpResult>(result);
            _mockSender.Verify(x => x.Send(It.IsAny<CreateSportCenterCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion CreateSportCenter Tests

        #region UpdateSportCenter Tests

        [Fact]
        public async Task UpdateSportCenter_Should_ReturnOk_When_Authenticated()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var sportCenterId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            SetupAuthorizedUser("SportCenterOwner", userId);

            var updateCommand = new UpdateSportCenterCommand(
                sportCenterId,                 // SportCenterId
                "Updated Sport Center",        // Name
                "0987654321",                  // PhoneNumber
                "Đường mới 123",               // AddressLine
                "Thành phố mới",               // City
                "Quận mới",                    // District
                "Phường mới",                  // Commune
                21.03,                         // Latitude
                105.84,                        // Longitude
                "avatar.jpg",                  // Avatar
                new List<string> { "updated-image.jpg" }, // ImageUrls
                "Mô tả cập nhật"               // Description
            );

            var sportCenterDetail = new SportCenterDetailDTO(
                sportCenterId,          // Id
                userId,                 // OwnerId
                "Updated Sport Center", // Name
                "0987654321",           // PhoneNumber
                "Đường mới 123",        // AddressLine
                "Thành phố mới",        // City
                "Quận mới",             // District
                "Phường mới",           // Commune
                21.03,                  // Latitude
                105.84,                 // Longitude
                "avatar.jpg",           // Avatar
                new List<string> { "updated-image.jpg" }, // ImageUrls
                false,                  // IsDeleted
                "Mô tả cập nhật",       // Description
                now,                    // CreatedAt
                now                     // LastModified
            );

            _mockSender.Setup(x => x.Send(It.IsAny<UpdateSportCenterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateSportCenterResult(sportCenterDetail));

            // Act
            var result = await InvokeUpdateSportCenterDelegate(sportCenterId, updateCommand, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Ok<SportCenterDetailDTO>>(result);
            var okResult = (Ok<SportCenterDetailDTO>)result;
            Assert.Equal(sportCenterId, okResult.Value.Id);
            Assert.Equal("Updated Sport Center", okResult.Value.Name);

            _mockSender.Verify(x => x.Send(It.IsAny<UpdateSportCenterCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateSportCenter_Should_ReturnNotFound_When_SportCenterDoesNotExist()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            SetupAuthorizedUser("SportCenterOwner");

            var updateCommand = new UpdateSportCenterCommand(
                sportCenterId,                 // SportCenterId
                "Updated Sport Center",        // Name
                "0987654321",                  // PhoneNumber
                "Đường mới 123",               // AddressLine
                "Thành phố mới",               // City
                "Quận mới",                    // District
                "Phường mới",                  // Commune
                21.03,                         // Latitude
                105.84,                        // Longitude
                "avatar.jpg",                  // Avatar
                new List<string> { "updated-image.jpg" }, // ImageUrls
                "Mô tả cập nhật"               // Description
            );

            _mockSender.Setup(x => x.Send(It.IsAny<UpdateSportCenterCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException($"Không tìm thấy trung tâm thể thao với ID {sportCenterId}"));

            // Act
            var result = await InvokeUpdateSportCenterDelegate(sportCenterId, updateCommand, _mockHttpContext.Object);

            // Assert
            Assert.IsType<NotFound>(result);
        }

        [Fact]
        public async Task UpdateSportCenter_Should_ReturnBadRequest_When_InvalidData()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            SetupAuthorizedUser("SportCenterOwner");

            var updateCommand = new UpdateSportCenterCommand(
                sportCenterId,                 // SportCenterId
                "",                           // Name - trống
                "invalid-phone",                // PhoneNumber - không hợp lệ
                "",                           // AddressLine - trống
                "",                           // City - trống
                "",                           // District - trống
                "",                           // Commune - trống
                0,                            // Latitude - 0
                0,                            // Longitude - 0
                "",                           // Avatar - trống
                new List<string>(),           // ImageUrls - trống
                ""                            // Description - trống
            );

            _mockSender.Setup(x => x.Send(It.IsAny<UpdateSportCenterCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Dữ liệu không hợp lệ"));

            // Act
            var result = await InvokeUpdateSportCenterDelegate(sportCenterId, updateCommand, _mockHttpContext.Object);

            // Assert
            Assert.IsType<BadRequest<ProblemDetails>>(result);
        }

        [Fact]
        public async Task UpdateSportCenter_Should_ReturnUnauthorized_When_NotAuthenticated()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            SetupUnauthenticatedUser();

            var updateCommand = new UpdateSportCenterCommand(
                sportCenterId,                 // SportCenterId
                "Updated Sport Center",        // Name
                "0987654321",                  // PhoneNumber
                "Đường mới 123",               // AddressLine
                "Thành phố mới",               // City
                "Quận mới",                    // District
                "Phường mới",                  // Commune
                21.03,                         // Latitude
                105.84,                        // Longitude
                "avatar.jpg",                  // Avatar
                new List<string> { "updated-image.jpg" }, // ImageUrls
                "Mô tả cập nhật"               // Description
            );

            // Act
            var result = await InvokeUpdateSportCenterDelegate(sportCenterId, updateCommand, _mockHttpContext.Object);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
            _mockSender.Verify(x => x.Send(It.IsAny<UpdateSportCenterCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateSportCenter_Should_ReturnForbidden_When_NotOwner()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            SetupAuthorizedUser("SportCenterOwner");

            var updateCommand = new UpdateSportCenterCommand(
                sportCenterId,                 // SportCenterId
                "Updated Sport Center",        // Name
                "0987654321",                  // PhoneNumber
                "Đường mới 123",               // AddressLine
                "Thành phố mới",               // City
                "Quận mới",                    // District
                "Phường mới",                  // Commune
                21.03,                         // Latitude
                105.84,                        // Longitude
                "avatar.jpg",                  // Avatar
                new List<string> { "updated-image.jpg" }, // ImageUrls
                "Mô tả cập nhật"               // Description
            );

            _mockSender.Setup(x => x.Send(It.IsAny<UpdateSportCenterCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedAccessException("Bạn không có quyền cập nhật trung tâm thể thao này"));

            // Act
            var result = await InvokeUpdateSportCenterDelegate(sportCenterId, updateCommand, _mockHttpContext.Object);

            // Assert
            Assert.IsType<ForbidHttpResult>(result);
        }

        #endregion UpdateSportCenter Tests

        #region GetAllCourtsOfSportCenter Tests

        [Fact]
        public async Task GetAllCourtsOfSportCenter_Should_ReturnOk_When_Exists()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            var courts = new List<CourtDTO>
            {
                new CourtDTO(
                    Guid.NewGuid(),              // Id
                    "Test Court 1",               // CourtName
                    Guid.NewGuid(),              // SportId
                    sportCenterId,               // SportCenterId
                    "Test Description",          // Description
                    new List<FacilityDTO>(),     // Facilities
                    TimeSpan.FromHours(1),       // SlotDuration
                    CourtStatus.Open,            // Status
                    CourtType.Indoor,            // CourtType
                    50m,                         // MinDepositPercentage
                    24,                          // CancellationWindowHours
                    100m,                        // RefundPercentage
                    "Tennis",                    // SportName
                    "Sport Center Test",         // SportCenterName
                    null,                        // Promotions
                    DateTime.UtcNow,             // CreatedAt
                    null                         // LastModified
                ),
                new CourtDTO(
                    Guid.NewGuid(),              // Id
                    "Test Court 2",               // CourtName
                    Guid.NewGuid(),              // SportId
                    sportCenterId,               // SportCenterId
                    "Test Description 2",         // Description
                    new List<FacilityDTO>(),     // Facilities
                    TimeSpan.FromHours(1),       // SlotDuration
                    CourtStatus.Open,            // Status
                    CourtType.Outdoor,           // CourtType
                    50m,                         // MinDepositPercentage
                    24,                          // CancellationWindowHours
                    100m,                        // RefundPercentage
                    "Basketball",                // SportName
                    "Sport Center Test",         // SportCenterName
                    null,                        // Promotions
                    DateTime.UtcNow,             // CreatedAt
                    null                         // LastModified
                )
            };

            SetupAuthorizedUser("User");

            _mockSender.Setup(x => x.Send(
                It.Is<GetAllCourtsOfSportCenterQuery>(q => q.SportCenterId == sportCenterId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAllCourtsOfSportCenterResult(courts));

            // Act
            var result = await InvokeGetAllCourtsOfSportCenterDelegate(sportCenterId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Ok<GetAllCourtsOfSportCenterResponse>>(result);
            var okResult = (Ok<GetAllCourtsOfSportCenterResponse>)result;
            Assert.Equal(2, okResult.Value.Courts.Count);
            Assert.Equal("Test Court 1", okResult.Value.Courts[0].CourtName);
            Assert.Equal("Test Court 2", okResult.Value.Courts[1].CourtName);
        }

        [Fact]
        public async Task GetAllCourtsOfSportCenter_Should_ReturnNotFound_When_SportCenterDoesNotExist()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            SetupAuthorizedUser("User");

            _mockSender.Setup(x => x.Send(
                It.Is<GetAllCourtsOfSportCenterQuery>(q => q.SportCenterId == sportCenterId),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException($"Không tìm thấy trung tâm thể thao với ID {sportCenterId}"));

            // Act
            var result = await InvokeGetAllCourtsOfSportCenterDelegate(sportCenterId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<NotFound>(result);
        }

        [Fact]
        public async Task GetAllCourtsOfSportCenter_Should_ReturnEmptyList_When_NoCourtFound()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            SetupAuthorizedUser("User");

            _mockSender.Setup(x => x.Send(
                It.Is<GetAllCourtsOfSportCenterQuery>(q => q.SportCenterId == sportCenterId),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAllCourtsOfSportCenterResult(new List<CourtDTO>()));

            // Act
            var result = await InvokeGetAllCourtsOfSportCenterDelegate(sportCenterId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Ok<GetAllCourtsOfSportCenterResponse>>(result);
            var okResult = (Ok<GetAllCourtsOfSportCenterResponse>)result;
            Assert.Empty(okResult.Value.Courts);
        }

        [Fact]
        public async Task GetAllCourtsOfSportCenter_Should_ReturnUnauthorized_When_NotAuthenticated()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            SetupUnauthenticatedUser();

            // Act
            var result = await InvokeGetAllCourtsOfSportCenterDelegate(sportCenterId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
            _mockSender.Verify(x => x.Send(It.IsAny<GetAllCourtsOfSportCenterQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion GetAllCourtsOfSportCenter Tests

        #region Helper Methods

        private void SetupAuthorizedUser(string role, Guid? userId = null)
        {
            var id = userId ?? Guid.NewGuid();
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims, "TestAuth"));
            _mockHttpContext.Setup(c => c.User).Returns(user);
        }

        private void SetupUnauthenticatedUser()
        {
            var unauthenticatedUser = new ClaimsPrincipal(new ClaimsIdentity());
            _mockHttpContext.Setup(c => c.User).Returns(unauthenticatedUser);
        }

        private async Task<IResult> InvokeGetSportCentersDelegate(
            int page, int limit, string city, string name)
        {
            return await ((Func<int, int, string, string, ISender, Task<IResult>>)(
                async (p, l, c, n, sender) =>
                {
                    try
                    {
                        var paginationRequest = new PaginationRequest(p - 1, l);
                        var query = new GetSportCentersQuery(paginationRequest, c, n);
                        var result = await sender.Send(query);

                        // Tạo GetSportCentersResponse với SportCenterListDTO
                        var response = new GetSportCentersResponse(result.SportCenters);
                        return Results.Ok(response);
                    }
                    catch (ValidationException ex)
                    {
                        return Results.BadRequest(new ProblemDetails
                        {
                            Title = "Validation Error",
                            Detail = ex.Message
                        });
                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest(new ProblemDetails
                        {
                            Title = "Error",
                            Detail = ex.Message
                        });
                    }
                }
            ))(page, limit, city, name, _mockSender.Object);
        }

        private async Task<IResult> InvokeGetSportCenterByIdDelegate(Guid id, HttpContext httpContext)
        {
            return await ((Func<Guid, HttpContext, ISender, Task<IResult>>)(
                async (centerId, ctx, sender) =>
                {
                    try
                    {
                        var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier);
                        if (userIdClaim == null)
                            return Results.Unauthorized();

                        var query = new GetSportCenterByIdQuery(centerId);
                        var result = await sender.Send(query);
                        return Results.Ok(result.SportCenter);
                    }
                    catch (KeyNotFoundException)
                    {
                        return Results.NotFound();
                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest(new ProblemDetails
                        {
                            Title = "Error",
                            Detail = ex.Message
                        });
                    }
                }
            ))(id, httpContext, _mockSender.Object);
        }

        private async Task<IResult> InvokeCreateSportCenterDelegate(
            CreateSportCenterCommand command, HttpContext httpContext)
        {
            return await ((Func<CreateSportCenterCommand, HttpContext, ISender, Task<IResult>>)(
                async (cmd, ctx, sender) =>
        {
            try
            {
                var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = ctx.User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null || roleClaim == null)
                    return Results.Unauthorized();

                var role = roleClaim.Value;
                if (role != "Admin" && role != "SportCenterOwner")
                    return Results.Forbid();

                var result = await sender.Send(cmd);
                var response = new CreateSportCenterResponse(result.Id);
                return Results.Created($"/api/sportcenters/{response.Id}", response);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Error",
                    Detail = ex.Message
                });
            }
        }
            ))(command, httpContext, _mockSender.Object);
        }

        private async Task<IResult> InvokeUpdateSportCenterDelegate(
            Guid id, UpdateSportCenterCommand command, HttpContext httpContext)
        {
            return await ((Func<Guid, UpdateSportCenterCommand, HttpContext, ISender, Task<IResult>>)(
                async (centerId, cmd, ctx, sender) =>
        {
            try
            {
                var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = ctx.User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null || roleClaim == null)
                    return Results.Unauthorized();

                // Thay vì: cmd.SportCenterId = centerId;
                // Tạo lại command với centerId từ path
                var updatedCommand = new UpdateSportCenterCommand(
                    centerId,                 // SportCenterId từ path
                    cmd.Name,                 // giữ nguyên các thông tin khác
                    cmd.PhoneNumber,
                    cmd.AddressLine,
                    cmd.City,
                    cmd.District,
                    cmd.Commune,
                    cmd.Latitude,
                    cmd.Longitude,
                    cmd.Avatar,
                    cmd.ImageUrls,
                    cmd.Description
                );

                var result = await sender.Send(updatedCommand);
                return Results.Ok(result.SportCenter);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = ex.Message
                });
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Error",
                    Detail = ex.Message
                });
            }
        }
            ))(id, command, httpContext, _mockSender.Object);
        }

        private async Task<IResult> InvokeGetAllCourtsOfSportCenterDelegate(
            Guid id, HttpContext httpContext)
        {
            return await ((Func<Guid, HttpContext, ISender, Task<IResult>>)(
                async (centerId, ctx, sender) =>
                {
                    try
                    {
                        var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier);
                        if (userIdClaim == null)
                            return Results.Unauthorized();

                        var result = await sender.Send(new GetAllCourtsOfSportCenterQuery(centerId));
                        var response = new GetAllCourtsOfSportCenterResponse(result.Courts);
                        return Results.Ok(response);
                    }
                    catch (KeyNotFoundException)
                    {
                        return Results.NotFound();
                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest(new ProblemDetails
                        {
                            Title = "Error",
                            Detail = ex.Message
                        });
                    }
                }
            ))(id, httpContext, _mockSender.Object);
        }

        #endregion Helper Methods
    }

    // 1. Tạo một lớp GetSportCentersResponse để sử dụng trong kiểm thử, phù hợp với SportCenterListDTO
    public record GetSportCentersResponse(PaginatedResult<SportCenterListDTO> SportCenters);
}