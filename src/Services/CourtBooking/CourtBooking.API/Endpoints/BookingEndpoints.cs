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

namespace CourtBooking.API.Endpoints
{
    public record CreateBookingRequest(BookingCreateDTO Booking);
    public record CreateBookingResponse(Guid Id);
    public record GetBookingDetailResponse(BookingDetailDto Booking);
    public record GetUserBookingsRequest(int Page = 1, int PageSize = 10, DateTime? StartDate = null, DateTime? EndDate = null, int? Status = null);
    public record GetUserBookingsResponse(PaginatedResult<BookingDto> Bookings);
    public record UpdateBookingStatusRequest(int Status);
    public record UpdateBookingStatusResponse(bool IsSuccess);

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

                var command = new CreateBookingCommand(request.Booking with { UserId = Guid.Parse(userId) });
                var result = await sender.Send(command);
                var response = new CreateBookingResponse(result.Id);
                return Results.Created($"/api/bookings/{response.Id}", response);
            })
            .WithName("CreateBooking")
            .Produces<CreateBookingResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Tạo đặt sân mới")
            .WithDescription("Tạo một đơn đặt sân mới cho người dùng hiện tại");

            group.MapGet("/", async (
                HttpContext httpContext,
                [FromServices] ISender sender,
                [FromQuery] Guid? user_id,
                [FromQuery] Guid? court_id,
                [FromQuery] Guid? sports_center_id,
                [FromQuery] BookingStatus? status,
                [FromQuery] DateTime? start_date,
                [FromQuery] DateTime? end_date,
                [FromQuery] int page = 0,
                [FromQuery] int limit = 10) =>
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
            })
            .RequireAuthorization()
            .WithName("GetBookingById")
            .Produces<BookingDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Booking By Id")
            .WithDescription("Get details of a specific booking if authorized");

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
        }
    }
}