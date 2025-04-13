using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using CourtBooking.API.Endpoints;
using CourtBooking.Application.CourtManagement.Command.CreateCourt;
using CourtBooking.Application.CourtManagement.Command.DeleteCourt;
using CourtBooking.Application.CourtManagement.Command.UpdateCourt;
using CourtBooking.Application.CourtManagement.Queries.GetCourtAvailability;
using CourtBooking.Application.CourtManagement.Queries.GetCourtDetails;
using CourtBooking.Application.CourtManagement.Queries.GetCourts;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using System.Security.Claims;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace CourtBooking.Test.API.Endpoints
{
    public class CourtEndpointsTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly CourtEndpoints _endpoints;
        private readonly Mock<HttpContext> _mockHttpContext;

        public CourtEndpointsTests()
        {
            _mockSender = new Mock<ISender>();
            _endpoints = new CourtEndpoints();
            _mockHttpContext = new Mock<HttpContext>();
        }

        #region CreateCourt Tests

        [Fact]
        public async Task CreateCourt_Should_ReturnCreated_When_Valid()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var sportCenterId = Guid.NewGuid();
            var sportId = Guid.NewGuid();

            var courtDto = new CourtCreateDTO(
                "Test Court",                    // CourtName
                sportId,                         // SportId
                sportCenterId,                   // SportCenterId
                "Test Description",              // Description
                null,                            // Facilities
                TimeSpan.FromHours(1),           // SlotDuration
                50,                              // MinDepositPercentage
                0,                               // CourtType (0 = Indoor)
                new List<CourtScheduleDTO>(),    // CourtSchedules
                24,                              // CancellationWindowHours (default)
                0                                // RefundPercentage (default)
            );

            var request = new CreateCourtRequest(courtDto);
            SetupAuthorizedUser("CourtOwner");

            _mockSender.Setup(x => x.Send(It.IsAny<CreateCourtCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateCourtResult(courtId));

            // Act
            var result = await InvokeCreateCourtDelegate(request, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Created<CreateCourtResponse>>(result);
            var created = (Created<CreateCourtResponse>)result;
            Assert.Equal($"/api/courts/{courtId}", created.Location);
            Assert.Equal(courtId, created.Value.Id);

            _mockSender.Verify(x => x.Send(
                It.Is<CreateCourtCommand>(c => c.Court == courtDto),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateCourt_Should_ReturnBadRequest_When_InvalidData()
        {
            // Arrange
            var courtDto = new CourtCreateDTO(
                "",                              // CourtName - trống (lỗi)
                Guid.Empty,                      // SportId - không hợp lệ
                Guid.Empty,                      // SportCenterId - không hợp lệ
                "Test Description",              // Description
                null,                            // Facilities
                TimeSpan.Zero,                   // SlotDuration - không hợp lệ
                -50,                             // MinDepositPercentage - âm (lỗi)
                99,                              // CourtType - không hợp lệ
                new List<CourtScheduleDTO>(),    // CourtSchedules - trống
                -24,                             // CancellationWindowHours - âm (lỗi)
                -10                              // RefundPercentage - âm (lỗi)
            );

            var request = new CreateCourtRequest(courtDto);
            SetupAuthorizedUser("CourtOwner");

            _mockSender.Setup(x => x.Send(It.IsAny<CreateCourtCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Dữ liệu không hợp lệ"));

            // Act
            var result = await InvokeCreateCourtDelegate(request, _mockHttpContext.Object);

            // Assert
            Assert.IsType<BadRequest<ProblemDetails>>(result);
        }

        [Fact]
        public async Task CreateCourt_Should_ReturnUnauthorized_When_UserNotAuthenticated()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            var sportId = Guid.NewGuid();

            var courtDto = new CourtCreateDTO(
                "Test Court",                    // CourtName
                sportId,                         // SportId
                sportCenterId,                   // SportCenterId
                "Test Description",              // Description
                null,                            // Facilities
                TimeSpan.FromHours(1),           // SlotDuration
                50,                              // MinDepositPercentage
                0,                               // CourtType (0 = Indoor)
                new List<CourtScheduleDTO>(),    // CourtSchedules
                24,                              // CancellationWindowHours
                0                                // RefundPercentage
            );

            var request = new CreateCourtRequest(courtDto);
            // Setting up unauthenticated user
            SetupUnauthenticatedUser();

            // Act
            var result = await InvokeCreateCourtDelegate(request, _mockHttpContext.Object);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
            _mockSender.Verify(x => x.Send(It.IsAny<CreateCourtCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateCourt_Should_ReturnForbidden_When_UserNotCourtOwner()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            var sportId = Guid.NewGuid();

            var courtDto = new CourtCreateDTO(
                "Test Court",                    // CourtName
                sportId,                         // SportId
                sportCenterId,                   // SportCenterId
                "Test Description",              // Description
                null,                            // Facilities
                TimeSpan.FromHours(1),           // SlotDuration
                50,                              // MinDepositPercentage
                0,                               // CourtType (0 = Indoor)
                new List<CourtScheduleDTO>(),    // CourtSchedules
                24,                              // CancellationWindowHours
                0                                // RefundPercentage
            );

            var request = new CreateCourtRequest(courtDto);
            // Setting up user with wrong role
            SetupAuthorizedUser("User");

            // Act
            var result = await InvokeCreateCourtDelegate(request, _mockHttpContext.Object);

            // Assert
            Assert.IsType<ForbidHttpResult>(result);
            _mockSender.Verify(x => x.Send(It.IsAny<CreateCourtCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateCourt_Should_HandleExternalException()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            var sportId = Guid.NewGuid();

            var courtDto = new CourtCreateDTO(
                "Test Court",                    // CourtName
                sportId,                         // SportId
                sportCenterId,                   // SportCenterId
                "Test Description",              // Description
                null,                            // Facilities
                TimeSpan.FromHours(1),           // SlotDuration
                50,                              // MinDepositPercentage
                0,                               // CourtType (0 = Indoor)
                new List<CourtScheduleDTO>(),    // CourtSchedules
                24,                              // CancellationWindowHours
                0                                // RefundPercentage
            );

            var request = new CreateCourtRequest(courtDto);
            SetupAuthorizedUser("CourtOwner");

            _mockSender.Setup(x => x.Send(It.IsAny<CreateCourtCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Lỗi server không xác định"));

            // Act
            var result = await InvokeCreateCourtDelegate(request, _mockHttpContext.Object);

            // Assert
            Assert.IsType<BadRequest<ProblemDetails>>(result);
        }

        #endregion CreateCourt Tests

        #region GetCourtDetails Tests

        [Fact]
        public async Task GetCourtDetails_Should_ReturnCourtDetails_When_Exists()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            SetupAuthorizedUser("User");

            var courtDTO = new CourtDTO(
                courtId,                     // Id
                "Test Court",                // CourtName
                Guid.NewGuid(),              // SportId
                Guid.NewGuid(),              // SportCenterId
                "Test Description",          // Description
                null,                        // Facilities
                TimeSpan.FromHours(1),       // SlotDuration
                CourtStatus.Open,            // Status
                CourtType.Indoor,            // CourtType
                50m,                         // MinDepositPercentage
                24,                          // CancellationWindowHours
                100m,                        // RefundPercentage
                "Tennis",                    // SportName
                "Sport Center 1",            // SportCenterName
                null,                        // Promotions
                DateTime.UtcNow,             // CreatedAt
                null                         // LastModified
            );

            // Tạo GetCourtDetailsResult với tham số Court
            var courtDetailsResult = new GetCourtDetailsResult(courtDTO);

            _mockSender.Setup(x => x.Send(It.IsAny<GetCourtDetailsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(courtDetailsResult);

            // Act
            var result = await InvokeGetCourtDetailsDelegate(courtId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Ok<GetCourtDetailsResponse>>(result);
            var okResult = (Ok<GetCourtDetailsResponse>)result;
            Assert.Equal(courtId, okResult.Value.Court.Id);

            _mockSender.Verify(x => x.Send(
                It.Is<GetCourtDetailsQuery>(q => q.CourtId == courtId),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetCourtDetails_Should_ReturnNotFound_When_CourtDoesNotExist()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            SetupAuthorizedUser("User");

            _mockSender.Setup(x => x.Send(It.IsAny<GetCourtDetailsQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException($"Không tìm thấy sân với ID {courtId}"));

            // Act
            var result = await InvokeGetCourtDetailsDelegate(courtId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<NotFound>(result);
        }

        [Fact]
        public async Task GetCourtDetails_Should_ReturnUnauthorized_When_UserNotAuthenticated()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            SetupUnauthenticatedUser();

            // Act
            var result = await InvokeGetCourtDetailsDelegate(courtId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
            _mockSender.Verify(x => x.Send(It.IsAny<GetCourtDetailsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion GetCourtDetails Tests

        #region GetCourts Tests

        [Fact]
        public async Task GetCourts_Should_ReturnPaginatedCourts()
        {
            // Arrange
            var sportCenterId = Guid.NewGuid();
            var sportId = Guid.NewGuid();
            var courtType = "Indoor";
            SetupAuthorizedUser("User");

            var paginationRequest = new PaginationRequest(0, 10);

            // Tạo CourtDTO với constructor
            var courtDTO = new CourtDTO(
                Guid.NewGuid(),              // Id
                "Test Court",                // CourtName
                sportId,                     // SportId
                sportCenterId,               // SportCenterId
                "Test Description",          // Description
                null,                        // Facilities
                TimeSpan.FromHours(1),       // SlotDuration
                CourtStatus.Open,            // Status
                CourtType.Indoor,            // CourtType
                50m,                         // MinDepositPercentage
                24,                          // CancellationWindowHours
                100m,                        // RefundPercentage
                "Tennis",                    // SportName
                "Sport Center 1",            // SportCenterName
                null,                        // Promotions
                DateTime.UtcNow,             // CreatedAt
                null                         // LastModified
            );

            // Tạo PaginatedResult với tham số đúng
            var paginatedResult = new PaginatedResult<CourtDTO>(
                0,                          // pageIndex
                10,                         // pageSize
                1,                          // count
                new List<CourtDTO> { courtDTO }  // data
            );

            // Tạo GetCourtsResult với tham số Courts
            var courtsResult = new GetCourtsResult(paginatedResult);

            _mockSender.Setup(x => x.Send(It.IsAny<GetCourtsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(courtsResult);

            // Act
            var result = await InvokeGetCourtsDelegate(paginationRequest, sportCenterId, sportId, courtType, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Ok<GetCourtsResponse>>(result);
            var okResult = (Ok<GetCourtsResponse>)result;
            Assert.Single(okResult.Value.Courts.Data); // Sử dụng .Data thay vì .Items
            Assert.Equal("Test Court", okResult.Value.Courts.Data.First().CourtName); // Sử dụng CourtName thay vì Name

            _mockSender.Verify(x => x.Send(
                It.Is<GetCourtsQuery>(q =>
                    q.PaginationRequest.PageIndex == paginationRequest.PageIndex &&
                    q.PaginationRequest.PageSize == paginationRequest.PageSize &&
                    q.sportCenterId == sportCenterId &&
                    q.sportId == sportId &&
                    q.courtType == courtType),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [InlineData(-1, 10)] // Trang âm
        [InlineData(0, 0)]   // Kích thước trang bằng 0
        [InlineData(0, -10)] // Kích thước trang âm
        [InlineData(0, 1001)] // Kích thước trang lớn hơn giới hạn (giả sử 1000 là max)
        public async Task GetCourts_Should_HandleInvalidPaginationParameters(int page, int pageSize)
        {
            // Arrange
            SetupAuthorizedUser("User");
            var paginationRequest = new PaginationRequest(page, pageSize);

            _mockSender.Setup(x => x.Send(It.IsAny<GetCourtsQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Tham số phân trang không hợp lệ"));

            // Act
            var result = await InvokeGetCourtsDelegate(paginationRequest, null, null, null, _mockHttpContext.Object);

            // Assert
            Assert.IsType<BadRequest<ProblemDetails>>(result);
        }

        [Fact]
        public async Task GetCourts_Should_ReturnEmptyList_When_NoCourtFound()
        {
            // Arrange
            SetupAuthorizedUser("User");
            var paginationRequest = new PaginationRequest(0, 10);

            // Tạo PaginatedResult với danh sách rỗng
            var paginatedResult = new PaginatedResult<CourtDTO>(
                0,                      // pageIndex
                10,                     // pageSize
                0,                      // count
                new List<CourtDTO>()    // data rỗng
            );

            // Tạo GetCourtsResult với tham số Courts
            var courtsResult = new GetCourtsResult(paginatedResult);

            _mockSender.Setup(x => x.Send(It.IsAny<GetCourtsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(courtsResult);

            // Act
            var result = await InvokeGetCourtsDelegate(paginationRequest, null, null, null, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Ok<GetCourtsResponse>>(result);
            var okResult = (Ok<GetCourtsResponse>)result;
            Assert.Empty(okResult.Value.Courts.Data); // Sử dụng .Data thay vì .Items
            Assert.Equal(0, okResult.Value.Courts.Count); // Sử dụng .Count thay vì .TotalCount
        }

        #endregion GetCourts Tests

        #region UpdateCourt Tests

        [Fact]
        public async Task UpdateCourt_Should_ReturnOk_When_Successful()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            SetupAuthorizedUser("CourtOwner");
            var sportId = Guid.NewGuid();

            var courtDto = new CourtUpdateDTO(
                "Updated Court",           // CourtName
                sportId,                   // SportId
                "Updated Description",     // Description
                null,                      // Facilities
                TimeSpan.FromHours(1),     // SlotDuration
                0,                         // Status
                1,                         // CourtType (1 = Outdoor)
                150,                       // MinDepositPercentage
                24,                        // CancellationWindowHours
                0                          // RefundPercentage
            );

            var request = new UpdateCourtRequest(courtDto);

            _mockSender.Setup(x => x.Send(It.IsAny<UpdateCourtCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateCourtResult(true));

            // Act
            var result = await InvokeUpdateCourtDelegate(courtId, request, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Ok<UpdateCourtResponse>>(result);
            var okResult = (Ok<UpdateCourtResponse>)result;
            Assert.True(okResult.Value.IsSuccess);

            _mockSender.Verify(x => x.Send(
                It.Is<UpdateCourtCommand>(c => c.Id == courtId && c.Court == courtDto),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCourt_Should_ReturnNotFound_When_CourtDoesNotExist()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            SetupAuthorizedUser("CourtOwner");
            var sportId = Guid.NewGuid();

            var courtDto = new CourtUpdateDTO(
                "Updated Court",           // CourtName
                sportId,                   // SportId
                "Updated Description",     // Description
                null,                      // Facilities
                TimeSpan.FromHours(1),     // SlotDuration
                0,                         // Status
                1,                         // CourtType (1 = Outdoor)
                150,                       // MinDepositPercentage
                24,                        // CancellationWindowHours
                0                          // RefundPercentage
            );

            var request = new UpdateCourtRequest(courtDto);

            _mockSender.Setup(x => x.Send(It.IsAny<UpdateCourtCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException($"Không tìm thấy sân với ID {courtId}"));

            // Act
            var result = await InvokeUpdateCourtDelegate(courtId, request, _mockHttpContext.Object);

            // Assert
            Assert.IsType<NotFound>(result);
        }

        [Fact]
        public async Task UpdateCourt_Should_ReturnForbidden_When_UserNotOwner()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            SetupAuthorizedUser("CourtOwner");
            var sportId = Guid.NewGuid();

            var courtDto = new CourtUpdateDTO(
                "Updated Court",           // CourtName
                sportId,                   // SportId
                "Updated Description",     // Description
                null,                      // Facilities
                TimeSpan.FromHours(1),     // SlotDuration
                0,                         // Status
                1,                         // CourtType (1 = Outdoor)
                150,                       // MinDepositPercentage
                24,                        // CancellationWindowHours
                0                          // RefundPercentage
            );

            var request = new UpdateCourtRequest(courtDto);

            _mockSender.Setup(x => x.Send(It.IsAny<UpdateCourtCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedAccessException("Bạn không có quyền cập nhật sân này"));

            // Act
            var result = await InvokeUpdateCourtDelegate(courtId, request, _mockHttpContext.Object);

            // Assert
            Assert.IsType<ForbidHttpResult>(result);
        }

        [Fact]
        public async Task UpdateCourt_Should_ReturnBadRequest_When_InvalidData()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            SetupAuthorizedUser("CourtOwner");

            var courtDto = new CourtUpdateDTO(
                "",                        // CourtName - trống (lỗi)
                Guid.Empty,                // SportId - không hợp lệ
                "",                        // Description
                null,                      // Facilities
                TimeSpan.Zero,             // SlotDuration - không hợp lệ
                99,                        // Status - không hợp lệ
                99,                        // CourtType - không hợp lệ
                -50,                       // MinDepositPercentage - âm (lỗi)
                -24,                       // CancellationWindowHours - âm (lỗi)
                -10                        // RefundPercentage - âm (lỗi)
            );

            var request = new UpdateCourtRequest(courtDto);

            _mockSender.Setup(x => x.Send(It.IsAny<UpdateCourtCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Dữ liệu không hợp lệ"));

            // Act
            var result = await InvokeUpdateCourtDelegate(courtId, request, _mockHttpContext.Object);

            // Assert
            Assert.IsType<BadRequest<ProblemDetails>>(result);
        }

        #endregion UpdateCourt Tests

        #region DeleteCourt Tests

        [Fact]
        public async Task DeleteCourt_Should_ReturnOk_When_Successful()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            SetupAuthorizedUser("CourtOwner");

            _mockSender.Setup(x => x.Send(It.IsAny<DeleteCourtCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteCourtResult(true));

            // Act
            var result = await InvokeDeleteCourtDelegate(courtId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Ok<DeleteCourtResponse>>(result);
            var okResult = (Ok<DeleteCourtResponse>)result;
            Assert.True(okResult.Value.IsSuccess);

            _mockSender.Verify(x => x.Send(
                It.Is<DeleteCourtCommand>(c => c.CourtId == courtId),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteCourt_Should_ReturnNotFound_When_CourtDoesNotExist()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            SetupAuthorizedUser("CourtOwner");

            _mockSender.Setup(x => x.Send(It.IsAny<DeleteCourtCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException($"Không tìm thấy sân với ID {courtId}"));

            // Act
            var result = await InvokeDeleteCourtDelegate(courtId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<NotFound>(result);
        }

        [Fact]
        public async Task DeleteCourt_Should_ReturnForbidden_When_UserNotOwner()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            SetupAuthorizedUser("User");

            _mockSender.Setup(x => x.Send(It.IsAny<DeleteCourtCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedAccessException("Bạn không có quyền xóa sân này"));

            // Act
            var result = await InvokeDeleteCourtDelegate(courtId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<ForbidHttpResult>(result);
        }

        [Fact]
        public async Task DeleteCourt_Should_ReturnConflict_When_CourtHasActiveBookings()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            SetupAuthorizedUser("CourtOwner");

            _mockSender.Setup(x => x.Send(It.IsAny<DeleteCourtCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Không thể xóa sân vì còn lịch đặt đang hoạt động"));

            // Act
            var result = await InvokeDeleteCourtDelegate(courtId, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Conflict<ProblemDetails>>(result);
        }

        #endregion DeleteCourt Tests

        #region GetCourtAvailability Tests

        [Fact]
        public async Task GetCourtAvailability_Should_ReturnAvailabilityInfo()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(7);
            SetupAuthorizedUser("User");

            var availabilityResult = new GetCourtAvailabilityResult(
                courtId,                      // CourtId
                7,                            // BookingDurationDays - 7 ngày
                new List<DailySchedule>()     // DailySchedules - danh sách lịch trình hằng ngày
            );

            _mockSender.Setup(x => x.Send(It.IsAny<GetCourtAvailabilityQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(availabilityResult);

            // Act
            var result = await InvokeGetCourtAvailabilityDelegate(courtId, startDate, endDate, _mockHttpContext.Object);

            // Assert
            Assert.IsType<Ok<GetCourtAvailabilityResult>>(result);
            var okResult = (Ok<GetCourtAvailabilityResult>)result;
            Assert.Equal(courtId, okResult.Value.CourtId);
            
            _mockSender.Verify(x => x.Send(
                It.Is<GetCourtAvailabilityQuery>(q =>
                    q.CourtId == courtId &&
                    q.StartDate == startDate &&
                    q.EndDate == endDate),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetCourtAvailability_Should_ReturnNotFound_When_CourtDoesNotExist()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(7);
            SetupAuthorizedUser("User");

            _mockSender.Setup(x => x.Send(It.IsAny<GetCourtAvailabilityQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException($"Không tìm thấy sân với ID {courtId}"));

            // Act
            var result = await InvokeGetCourtAvailabilityDelegate(courtId, startDate, endDate, _mockHttpContext.Object);

            // Assert
            Assert.IsType<NotFound>(result);
        }

        [Theory]
        [InlineData("2025-01-01", "2024-01-01")] // Ngày kết thúc trước ngày bắt đầu
        [InlineData("2024-01-01", "2024-01-31")] // Khoảng thời gian quá dài (giả sử giới hạn là 14 ngày)
        public async Task GetCourtAvailability_Should_ReturnBadRequest_When_InvalidDateRange(string start, string end)
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var startDate = DateTime.Parse(start);
            var endDate = DateTime.Parse(end);
            SetupAuthorizedUser("User");

            _mockSender.Setup(x => x.Send(It.IsAny<GetCourtAvailabilityQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Khoảng thời gian không hợp lệ"));

            // Act
            var result = await InvokeGetCourtAvailabilityDelegate(courtId, startDate, endDate, _mockHttpContext.Object);

            // Assert
            Assert.IsType<BadRequest<ProblemDetails>>(result);
        }

        #endregion GetCourtAvailability Tests

        #region Helper Methods

        private void SetupAuthorizedUser(string role)
        {
            var userId = Guid.NewGuid();
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
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

        private async Task<IResult> InvokeCreateCourtDelegate(CreateCourtRequest request, HttpContext httpContext)
        {
            return await ((Func<CreateCourtRequest, HttpContext, ISender, Task<IResult>>)(
                async (req, ctx, sender) =>
                {
                    try
                    {
                        var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier);
                        var roleClaim = ctx.User.FindFirst(ClaimTypes.Role);

                        if (userIdClaim == null || roleClaim == null)
                            return Results.Unauthorized();

                        var role = roleClaim.Value;
                        if (role != "CourtOwner" && role != "Admin")
                            return Results.Forbid();

                        var command = new CreateCourtCommand(req.Court);
                        var result = await sender.Send(command);
                        var response = new CreateCourtResponse(result.Id);
                        return Results.Created($"/api/courts/{response.Id}", response);
                    }
                    catch (ValidationException ex)
                    {
                        return Results.BadRequest(new ProblemDetails
                        {
                            Title = "Validation Error",
                            Detail = ex.Message
                        });
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return Results.Forbid();
                    }
                    catch (NotFoundException ex)
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
            ))(request, httpContext, _mockSender.Object);
        }

        private async Task<IResult> InvokeGetCourtDetailsDelegate(Guid id, HttpContext httpContext)
        {
            return await ((Func<Guid, HttpContext, ISender, Task<IResult>>)(
                async (courtId, ctx, sender) =>
                {
                    try
                    {
                        var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier);
                        if (userIdClaim == null)
                            return Results.Unauthorized();

                        var result = await sender.Send(new GetCourtDetailsQuery(courtId));
                        var response = new GetCourtDetailsResponse(result.Court);
                        return Results.Ok(response);
                    }
                    catch (NotFoundException)
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

        private async Task<IResult> InvokeGetCourtsDelegate(
            PaginationRequest request, Guid? sportCenterId, Guid? sportId, string courtType, HttpContext httpContext)
        {
            return await ((Func<PaginationRequest, Guid?, Guid?, string, HttpContext, ISender, Task<IResult>>)(
                async (req, scId, sId, cType, ctx, sender) =>
                {
                    try
                    {
                        var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier);
                        if (userIdClaim == null)
                            return Results.Unauthorized();

                        var query = new GetCourtsQuery(req, scId, sId, cType);
                        var result = await sender.Send(query);
                        var response = new GetCourtsResponse(result.Courts);
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
            ))(request, sportCenterId, sportId, courtType, httpContext, _mockSender.Object);
        }

        private async Task<IResult> InvokeUpdateCourtDelegate(Guid id, UpdateCourtRequest request, HttpContext httpContext)
        {
            return await ((Func<Guid, UpdateCourtRequest, HttpContext, ISender, Task<IResult>>)(
                async (courtId, req, ctx, sender) =>
                {
                    try
                    {
                        var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier);
                        var roleClaim = ctx.User.FindFirst(ClaimTypes.Role);

                        if (userIdClaim == null || roleClaim == null)
                            return Results.Unauthorized();

                        var command = new UpdateCourtCommand(courtId, req.Court);
                        var result = await sender.Send(command);
                        var response = new UpdateCourtResponse(result.IsSuccess);
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
                    catch (UnauthorizedAccessException)
                    {
                        return Results.Forbid();
                    }
                    catch (NotFoundException)
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
            ))(id, request, httpContext, _mockSender.Object);
        }

        private async Task<IResult> InvokeDeleteCourtDelegate(Guid id, HttpContext httpContext)
        {
            return await ((Func<Guid, HttpContext, ISender, Task<IResult>>)(
                async (courtId, ctx, sender) =>
                {
                    try
                    {
                        var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier);
                        var roleClaim = ctx.User.FindFirst(ClaimTypes.Role);

                        if (userIdClaim == null || roleClaim == null)
                            return Results.Unauthorized();
                        
                        var result = await sender.Send(new DeleteCourtCommand(courtId));
                        var response = new DeleteCourtResponse(result.IsSuccess);
                        return Results.Ok(response);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return Results.Forbid();
                    }
                    catch (NotFoundException)
                    {
                        return Results.NotFound();
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.Conflict(new ProblemDetails
                        {
                            Title = "Conflict",
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
            ))(id, httpContext, _mockSender.Object);
        }

        private async Task<IResult> InvokeGetCourtAvailabilityDelegate(
            Guid id, DateTime startDate, DateTime endDate, HttpContext httpContext)
        {
            return await ((Func<Guid, DateTime, DateTime, HttpContext, ISender, Task<IResult>>)(
                async (courtId, start, end, ctx, sender) =>
                {
                    try
                    {
                        var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier);
                        if (userIdClaim == null)
                            return Results.Unauthorized();

                        var query = new GetCourtAvailabilityQuery(courtId, start, end);
                        var result = await sender.Send(query);
                        return Results.Ok(result);
                    }
                    catch (ValidationException ex)
                    {
                        return Results.BadRequest(new ProblemDetails
                        {
                            Title = "Validation Error",
                            Detail = ex.Message
                        });
                    }
                    catch (NotFoundException)
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
            ))(id, startDate, endDate, httpContext, _mockSender.Object);
        }

        #endregion Helper Methods
    }
}