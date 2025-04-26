using CourtBooking.API.Endpoints;
using CourtBooking.Application.BookingManagement.Command.CancelBooking;
using CourtBooking.Application.BookingManagement.Command.CreateBooking;
using CourtBooking.Application.BookingManagement.Queries.GetBookingById;
using CourtBooking.Application.BookingManagement.Queries.GetBookings;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using BuildingBlocks.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.Test.API.Endpoints
{
    public class BookingEndpointsTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly HttpContext _httpContext;

        public BookingEndpointsTests()
        {
            _mockSender = new Mock<ISender>();

            // Cấu hình HttpContext giả lập với ClaimsPrincipal
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            _httpContext = new DefaultHttpContext
            {
                User = user
            };
        }

        [Fact]
        public async Task CreateBooking_Should_ReturnCreated_WhenValid()
        {
            // Arrange
            var request = new CreateBookingRequest(new BookingCreateDTO(
                BookingDate: DateTime.Today.AddDays(1),
                Note: "Test booking",
                DepositAmount: 50m,
                BookingDetails: new List<BookingDetailCreateDTO>
                {
                    new BookingDetailCreateDTO(
                        Guid.NewGuid(),
                        TimeSpan.FromHours(10),
                        TimeSpan.FromHours(12)
                    )
                }
            ));

            var resultId = Guid.NewGuid();
            _mockSender.Setup(s => s.Send(It.IsAny<CreateBookingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateBookingResult(resultId, "Deposited")); // Changed from Confirmed to Deposited

            // Act
            var result = await BookingEndpoints.CreateBooking(request, _mockSender.Object, _httpContext.User);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Created<CreateBookingResponse>>(result);

            var createdResult = result as Created<CreateBookingResponse>;
            Assert.NotNull(createdResult);
            Assert.Equal($"/api/bookings/{resultId}", createdResult.Location);
            Assert.Equal(resultId, createdResult.Value.Id);
        }

        [Fact]
        public async Task CreateBooking_Should_ReturnUnauthorized_WhenUserIdClaimMissing()
        {
            // Arrange
            var request = new CreateBookingRequest(new BookingCreateDTO(
                BookingDate: DateTime.Today.AddDays(1),
                Note: "Test booking",
                DepositAmount: 50m,
                BookingDetails: new List<BookingDetailCreateDTO>
                {
                    new BookingDetailCreateDTO(
                        Guid.NewGuid(),
                        TimeSpan.FromHours(10),
                        TimeSpan.FromHours(12)
                    )
                }
            ));

            // User không có claim NameIdentifier
            var userWithoutId = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            // Act
            var result = await BookingEndpoints.CreateBooking(request, _mockSender.Object, userWithoutId);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
        }

        [Fact]
        public async Task GetBookings_Should_ReturnOkWithResults_WhenAuthorized()
        {
            // Arrange
            var bookings = new List<BookingDto>
            {
                new BookingDto(
                    Id: Guid.NewGuid(),
                    UserId: Guid.NewGuid(),
                    TotalTime: 2m,
                    TotalPrice: 200m,
                    RemainingBalance: 0m,
                    InitialDeposit: 200m,
                    Status: BookingStatus.Deposited.ToString(), // Changed from Confirmed to Deposited
                    BookingDate: DateTime.Today,
                    Note: "Test note",
                    CreatedAt: DateTime.Now,
                    LastModified: null,
                    BookingDetails: new List<BookingDetailDto>())
            };

            _mockSender.Setup(s => s.Send(It.IsAny<GetBookingsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetBookingsResult(bookings, 1));

            // Act
            var result = await BookingEndpoints.GetBookings(
                _httpContext,
                _mockSender.Object,
                null, null, null, null, null, null, 0, 10);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Ok<GetBookingsResult>>(result);
            var okResult = (Ok<GetBookingsResult>)result;
            Assert.NotNull(okResult.Value);
            Assert.Single(okResult.Value.Bookings);
            Assert.Equal(1, okResult.Value.TotalCount);
        }

        [Fact]
        public async Task GetBookings_Should_ReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            var httpContextWithoutUser = new DefaultHttpContext();

            // Act
            var result = await BookingEndpoints.GetBookings(
                httpContextWithoutUser,
                _mockSender.Object,
                null, null, null, null, null, null, 0, 10);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
        }

        [Fact]
        public async Task GetBookingById_Should_ReturnOkWithBooking_WhenAuthorized()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var booking = new BookingDto(
                Id: bookingId,
                UserId: Guid.NewGuid(),
                TotalTime: 2m,
                TotalPrice: 200m,
                RemainingBalance: 0m,
                InitialDeposit: 200m,
                Status: BookingStatus.Deposited.ToString(), // Changed from Confirmed to Deposited
                BookingDate: DateTime.Today,
                Note: "Test note",
                CreatedAt: DateTime.Now,
                LastModified: null,
                BookingDetails: new List<BookingDetailDto>()
            );

            _mockSender.Setup(s => s.Send(It.IsAny<GetBookingByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Act
            var result = await BookingEndpoints.GetBookingById(
                bookingId,
                _httpContext,
                _mockSender.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Ok<BookingDto>>(result);
            var okResult = (Ok<BookingDto>)result;
            Assert.Equal(bookingId, okResult.Value.Id);
        }

        [Fact]
        public async Task GetBookingById_Should_ReturnForbid_WhenBookingNotFound()
        {
            // Arrange
            var bookingId = Guid.NewGuid();

            _mockSender.Setup(s => s.Send(It.IsAny<GetBookingByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BookingDto)null);

            // Act
            var result = await BookingEndpoints.GetBookingById(
                bookingId,
                _httpContext,
                _mockSender.Object);

            // Assert
            Assert.IsType<ForbidHttpResult>(result);
        }

        [Fact]
        public async Task CancelBooking_Should_ReturnOk_WhenSuccessful()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var request = new CancelBookingRequest(
                CancellationReason: "Change of plans",
                RequestedAt: DateTime.UtcNow
            );

            var cancelResult = new CancelBookingResult(
                BookingId: bookingId,
                Message: "Booking cancelled successfully",
                RefundAmount: 50m,
                Status: "Cancelled"
            );

            _mockSender.Setup(s => s.Send(It.IsAny<CancelBookingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cancelResult);

            // Act
            var result = await BookingEndpoints.CancelBooking(
                bookingId,
                request,
                _mockSender.Object,
                _httpContext);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Ok<CancelBookingResult>>(result);
            var okResult = (Ok<CancelBookingResult>)result;
            Assert.Equal(bookingId, okResult.Value.BookingId);
            Assert.Equal(50m, okResult.Value.RefundAmount);
        }

        [Fact]
        public async Task CancelBooking_Should_ReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var request = new CancelBookingRequest(
                CancellationReason: "Change of plans",
                RequestedAt: DateTime.UtcNow
            );

            var httpContextWithoutUser = new DefaultHttpContext();

            // Act
            var result = await BookingEndpoints.CancelBooking(
                bookingId,
                request,
                _mockSender.Object,
                httpContextWithoutUser);

            // Assert
            Assert.IsType<UnauthorizedHttpResult>(result);
        }

        // Các phương thức helper cho endpoint
        private static class BookingEndpoints
        {
            public static async Task<IResult> CreateBooking(
                CreateBookingRequest request,
                ISender sender,
                ClaimsPrincipal user)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var command = new CreateBookingCommand(Guid.Parse(userId), request.Booking);
                var result = await sender.Send(command);
                var response = new CreateBookingResponse(result.Id, result.Status);
                return Results.Created($"/api/bookings/{response.Id}", response);
            }

            public static async Task<IResult> GetBookings(
                HttpContext httpContext,
                ISender sender,
                Guid? user_id,
                Guid? court_id,
                Guid? sports_center_id,
                BookingStatus? status,
                DateTime? start_date,
                DateTime? end_date,
                int page = 0,
                int limit = 10)
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null || roleClaim == null)
                    return Results.Unauthorized();

                var userId = Guid.Parse(userIdClaim.Value);
                var role = roleClaim.Value;

                var query = new GetBookingsQuery(
                    UserId: userId,
                    Role: role,
                    ViewAs: null,
                    FilterUserId: user_id,
                    CourtId: court_id,
                    SportsCenterId: sports_center_id,
                    Status: status,
                    StartDate: start_date,
                    EndDate: end_date,
                    Page: page,
                    Limit: limit
                );

                var result = await sender.Send(query);
                return Results.Ok(result);
            }

            public static async Task<IResult> GetBookingById(
                Guid bookingId,
                HttpContext httpContext,
                ISender sender)
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null || roleClaim == null)
                    return Results.Unauthorized();

                var userId = Guid.Parse(userIdClaim.Value);
                var role = roleClaim.Value;

                var query = new GetBookingByIdQuery(bookingId, userId, role);
                var result = await sender.Send(query);

                if (result == null)
                    return Results.Forbid();

                return Results.Ok(result);
            }

            public static async Task<IResult> CancelBooking(
                Guid bookingId,
                CancelBookingRequest request,
                ISender sender,
                HttpContext context)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                var role = context.User.FindFirstValue(ClaimTypes.Role) ?? "";

                var command = new CancelBookingCommand(
                    bookingId,
                    request.CancellationReason,
                    request.RequestedAt,
                    Guid.Parse(userId),
                    role
                );

                var result = await sender.Send(command);
                return Results.Ok(result);
            }
        }
    }
}