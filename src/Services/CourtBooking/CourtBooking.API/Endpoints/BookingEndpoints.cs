using CourtBooking.Application.BookingManagement.Command.CreateBooking;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BuildingBlocks.Pagination;
using CourtBooking.Application.BookingManagement.Queries.GetBookings;
using CourtBooking.Domain.Enums;
using CourtBooking.Application.BookingManagement.Queries.GetBookingById;
using CourtBooking.Application.BookingManagement.Command.CancelBooking;
using CourtBooking.Application.BookingManagement.Command.UpdateBookingNote;
using CourtBooking.Application.BookingManagement.Command.UpdateBookingStatus;
using CourtBooking.Application.BookingManagement.Command.CancelBookingByOwner;
using CourtBooking.Application.BookingManagement.Queries.CalculateBookingPrice;
using Microsoft.IdentityModel.JsonWebTokens;
using CourtBooking.Application.BookingManagement.Command.CreateOwnerBooking;
using CourtBooking.Application.UserManagement.Queries.GetUserDashboard;

namespace CourtBooking.API.Endpoints
{
    public record CreateBookingRequest(BookingCreateDTO Booking);
    public record CreateBookingResponse(Guid Id, string Status);
    public record GetBookingDetailResponse(BookingDetailDto Booking);
    public record GetUserBookingsRequest(int Page = 1, int PageSize = 10, DateTime? StartDate = null, DateTime? EndDate = null, int? Status = null);
    public record GetUserBookingsResponse(PaginatedResult<BookingDto> Bookings);
    public record UpdateBookingStatusRequest(string Status);
    public record UpdateBookingStatusResponse(bool IsSuccess);
    public record CalculateBookingPriceRequest(BookingCreateDTO Booking);
    public record CalculateBookingPriceResponse(
        List<CourtPriceDetail> CourtPrices,
        decimal TotalPrice,
        decimal MinimumDeposit);

    public record CourtPriceDetail(
        Guid CourtId,
        string CourtName,
        TimeSpan StartTime,
        TimeSpan EndTime,
        decimal OriginalPrice,
        decimal DiscountedPrice,
        string? PromotionName,
        string? DiscountType,
        decimal? DiscountValue);

    public record CreateOwnerBookingRequest(BookingCreateDTO Booking, string Note = "Đặt trực tiếp tại sân");
    public record CreateOwnerBookingResponse(Guid Id, string Status, string Message);

    // New record for user dashboard response
    public record UserDashboardResponse(
        List<UpcomingBookingDto> UpcomingBookings,
        List<IncompleteTransactionDto> IncompleteTransactions,
        UserBookingStatsDto Stats
    );

    public record CancelBookingRequest(string CancellationReason, DateTime RequestedAt);
    public record CancelBookingByOwnerRequest(string CancellationReason, DateTime RequestedAt);

    public class BookingEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/bookings").WithTags("Booking");

            // Create Booking
            group.MapPost("/", [Authorize] async ([FromBody] CreateBookingRequest request, ISender sender, ClaimsPrincipal user) =>
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
            })
            .WithName("CreateBooking")
            .Produces<CreateBookingResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Tạo đặt sân mới")
            .WithDescription("Tạo một đơn đặt sân mới cho người dùng hiện tại");
            group.MapPost("/calculate", [Authorize] async ([FromBody] CalculateBookingPriceRequest request, ISender sender, ClaimsPrincipal user) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var query = new CalculateBookingPriceQuery(Guid.Parse(userId), request.Booking);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("CalculateBookingPrice")
            .Produces<CalculateBookingPriceResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Tính toán giá đặt sân")
            .WithDescription("Tính toán giá đặt sân trước khi đặt, bao gồm giá gốc, giá sau giảm giá, và số tiền đặt cọc tối thiểu");

            group.MapPost("/owner-booking", [Authorize(Policy = "CourtOwner")] async (
                [FromBody] CreateOwnerBookingRequest request,
                ISender sender,
                HttpContext httpContext) =>
            {
                var ownerIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(ownerIdClaim) || !Guid.TryParse(ownerIdClaim, out var ownerId))
                {
                    return Results.Unauthorized();
                }

                // Tạo command với ID của chủ sân
                var command = new CreateOwnerBookingCommand(ownerId, request.Booking, request.Note);
                var result = await sender.Send(command);

                if (!result.Success)
                {
                    return Results.BadRequest(new { message = result.Message });
                }

                return Results.Created($"/api/bookings/{result.Id}",
                    new CreateOwnerBookingResponse(result.Id, result.Status, result.Message));
            })
            .WithName("CreateOwnerBooking")
            .Produces<CreateOwnerBookingResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Đánh dấu sân đã đặt bởi chủ sân")
            .WithDescription("Cho phép chủ sân đánh dấu rằng một slot đã được đặt trực tiếp hoặc không khả dụng");

            group.MapGet("/", async (
                HttpContext httpContext,
                [FromServices] ISender sender,
                [FromQuery] string? view_as,
                [FromQuery] Guid? user_id,
                [FromQuery] Guid? court_id,
                [FromQuery] Guid? sports_center_id,
                [FromQuery] BookingStatus? status,
                [FromQuery] DateTime? start_date,
                [FromQuery] DateTime? end_date,
                [FromQuery] int page = 0,
                [FromQuery] int limit = 10) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                               ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                // Fix: Add null handling for roleClaim
                var role = roleClaim?.Value ?? "User";

                var query = new GetBookingsQuery(
                    UserId: userId,
                    Role: role,
                    ViewAs: view_as,
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
            })
            .RequireAuthorization()
            .WithName("GetBookings")
            .Produces<GetBookingsResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get Bookings")
            .WithDescription("Get a list of bookings based on filters and user role");

            group.MapGet("/{bookingId:guid}", async (
                Guid bookingId,
                HttpContext httpContext,
                [FromServices] ISender sender) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                               ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();
                var role = roleClaim?.Value ?? "User";

                var query = new GetBookingByIdQuery(bookingId, userId, role);
                var result = await sender.Send(query);

                if (result == null)
                    return Results.Forbid();

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetBookingById")
            .Produces<BookingDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Booking By Id")
            .WithDescription("Get details of a specific booking if authorized");
            // Add this endpoint in the AddRoutes method of CourtEndpoints.cs
            group.MapPut("/bookings/{bookingId:guid}/status", [Authorize(Policy = "CourtOwner")] async (
                Guid bookingId,
                [FromBody] UpdateBookingStatusRequest request,
                HttpContext httpContext,
                ISender sender) =>
            {
                // Extract owner ID from JWT claims
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var ownerId))
                {
                    return Results.Problem("Unable to identify user", statusCode: StatusCodes.Status401Unauthorized);
                }

                if (!Enum.TryParse<BookingStatus>(request.Status, true, out var bookingStatus))
                {
                    return Results.Problem("Invalid booking status", statusCode: StatusCodes.Status400BadRequest);
                }

                var command = new UpdateBookingStatusCommand(bookingId, ownerId, bookingStatus);
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(new UpdateBookingStatusResponse(result.IsSuccess))
                    : Results.Problem(result.ErrorMessage, statusCode: StatusCodes.Status400BadRequest);
            })
                        .WithName("UpdateBookingStatus")
                        .Produces<UpdateBookingStatusResponse>(StatusCodes.Status200OK)
                        .ProducesProblem(StatusCodes.Status400BadRequest)
                        .ProducesProblem(StatusCodes.Status401Unauthorized)
                        .WithSummary("Update Booking Status")
                        .WithDescription("Allows court owners to update the status of a booking for their courts.");

            // Add this endpoint in the AddRoutes method of CourtEndpoints.cs
            group.MapPut("/bookings/{bookingId:guid}/note", [Authorize(Policy = "User")] async (
                Guid bookingId,
                [FromBody] UpdateBookingNoteRequest request,
                HttpContext httpContext,
                ISender sender) =>
            {
                // Extract user ID from JWT claims
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Results.Problem("Unable to identify user", statusCode: StatusCodes.Status401Unauthorized);
                }

                var command = new UpdateBookingNoteCommand(bookingId, userId, request.Note);
                var result = await sender.Send(command);
                return result.IsSuccess
                    ? Results.Ok(new UpdateBookingNoteResponse(result.IsSuccess))
                    : Results.Problem(result.ErrorMessage, statusCode: StatusCodes.Status400BadRequest);
            })
            .WithName("UpdateBookingNote")
            .Produces<UpdateBookingNoteResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Update Booking Note")
            .WithDescription("Allows users to update the note field of their own bookings.");

            group.MapPut("/{bookingId:guid}/cancel", async (
                Guid bookingId,
                [FromBody] CancelBookingRequest request,
                ISender sender,
                HttpContext context) =>
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
            })
            .RequireAuthorization()
            .WithName("CancelBooking")
            .Produces<CancelBookingResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Cancel a booking")
            .WithDescription("Cancels a booking and processes refund if applicable based on the court's cancellation policy");

            // Add new endpoint for court owner booking cancellation with 100% refund
            group.MapPut("/{bookingId:guid}/cancel-by-owner", [Authorize(Policy = "CourtOwner")] async (
                Guid bookingId,
                [FromBody] CancelBookingByOwnerRequest request,
                ISender sender,
                HttpContext context) =>
            {
                var ownerId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(ownerId))
                    return Results.Unauthorized();

                var command = new CancelBookingByOwnerCommand(
                    bookingId,
                    request.CancellationReason,
                    request.RequestedAt,
                    Guid.Parse(ownerId)
                );

                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .WithName("CancelBookingByOwner")
            .Produces<CancelBookingByOwnerResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Cancel a booking by court owner")
            .WithDescription("Allows court owners to cancel a booking with 100% refund to the customer");

            // User Dashboard Endpoint
            group.MapGet("/dashboard", [Authorize] async (
                HttpContext httpContext,
                [FromServices] ISender sender) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                           ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var query = new GetUserDashboardQuery(userId);
                var result = await sender.Send(query);

                return Results.Ok(new UserDashboardResponse(
                    result.UpcomingBookings,
                    result.IncompleteTransactions,
                    result.Stats
                ));
            })
            .WithName("GetUserDashboard")
            .Produces<UserDashboardResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get User Dashboard")
            .WithDescription("Returns user dashboard information including upcoming bookings, incomplete transactions, and booking statistics");
        }
    }

    public record UpdateBookingNoteRequest(string Note);
    public record UpdateBookingNoteResponse(bool IsSuccess);
}