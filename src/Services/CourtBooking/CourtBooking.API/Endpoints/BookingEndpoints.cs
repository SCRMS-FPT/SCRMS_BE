using CourtBooking.Application.BookingManagement.Command.CreateBooking;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BuildingBlocks.Pagination;

namespace CourtBooking.API.Endpoints
{
    public record CreateBookingRequest(BookingCreateDTO Booking);
    public record CreateBookingResponse(Guid Id);
    public record GetBookingDetailResponse(BookingDetailDTO Booking);
    public record GetUserBookingsRequest(int Page = 1, int PageSize = 10, DateTime? StartDate = null, DateTime? EndDate = null, int? Status = null);
    public record GetUserBookingsResponse(PaginatedResult<BookingDTO> Bookings);
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
        }
    }
}