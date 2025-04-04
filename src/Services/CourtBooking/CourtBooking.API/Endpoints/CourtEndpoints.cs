﻿using BuildingBlocks.Pagination;
using CourtBooking.Application.CourtManagement.Command.CreateCourt;
using CourtBooking.Application.CourtManagement.Command.UpdateCourt;
using CourtBooking.Application.CourtManagement.Command.DeleteCourt;
using CourtBooking.Application.CourtManagement.Queries.GetCourts;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourtBooking.Application.CourtManagement.Queries.GetCourtAvailability;
using CourtBooking.Application.CourtManagement.Queries.GetCourtDetails;
using CourtBooking.Application.CourtManagement.Queries.GetCourtsByOwner;
using System.Security.Claims;
namespace CourtBooking.API.Endpoints
{
    public record CreateCourtRequest(CourtCreateDTO Court);
    public record CreateCourtResponse(Guid Id);
    public record GetCourtDetailsResponse(CourtDTO Court);
    public record GetCourtsResponse(PaginatedResult<CourtDTO> Courts);
    public record GetAllCourtsOfSportCenterResponse(List<CourtDTO> Courts);
    public record UpdateCourtRequest(CourtUpdateDTO Court);
    public record UpdateCourtResponse(bool IsSuccess);
    public record DeleteCourtResponse(bool IsSuccess);
    public record GetCourtsByOwnerResponse(PaginatedResult<CourtDTO> Courts);
    public class CourtEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/courts").WithTags("Court");

            // Create Court
            group.MapPost("/", [Authorize(Policy = "CourtOwner")] async ([FromBody] CreateCourtRequest request, ISender sender) =>
            {
                var command = new CreateCourtCommand(request.Court);
                var result = await sender.Send(command);
                var response = new CreateCourtResponse(result.Id);
                return Results.Created($"/api/courts/{response.Id}", response);
            })
            .WithName("CreateCourt")
            .Produces<CreateCourtResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Tạo sân mới")
            .WithDescription("Tạo một sân mới (yêu cầu quyền CourtOwner)");

            // Get Court Details
            group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetCourtDetailsQuery(id));
                var response = new GetCourtDetailsResponse(result.Court);
                return Results.Ok(response);
            })
            .WithName("GetCourtDetails")
            .Produces<GetCourtDetailsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Lấy chi tiết sân")
            .WithDescription("Lấy thông tin chi tiết của một sân cụ thể theo ID");

            // Get Courts
            group.MapGet("/", async ([AsParameters] PaginationRequest request,
                [FromQuery] Guid? sportCenterId, [FromQuery] Guid? sportId, [FromQuery] string? courtType, ISender sender) =>
            {
                var query = new GetCourtsQuery(request, sportCenterId, sportId, courtType);
                var result = await sender.Send(query);
                var response = new GetCourtsResponse(result.Courts);
                return Results.Ok(response);
            })
            .WithName("GetCourts")
            .Produces<GetCourtsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Lấy danh sách sân")
            .WithDescription("Lấy danh sách sân có phân trang và lọc theo trung tâm, môn thể thao và loại sân");
            group.MapGet("/owner-courts", [Authorize(Policy = "CourtOwner")] async (
                [AsParameters] PaginationRequest request,
                [FromQuery] Guid? sportId,
                [FromQuery] string? courtType,
                HttpContext httpContext,
                ISender sender) =>
            {
                // Extract owner ID from JWT claims
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var ownerId))
                {
                    return Results.Problem("Unable to identify user", statusCode: StatusCodes.Status401Unauthorized);
                }

                var query = new GetCourtsByOwnerQuery(request, ownerId, sportId, courtType);
                var result = await sender.Send(query);
                var response = new GetCourtsByOwnerResponse(result.Courts);
                return Results.Ok(response);
            })
            .WithName("GetCourtsByOwner")
            .Produces<GetCourtsByOwnerResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Lấy danh sách sân thuộc sở hữu của chủ sân")
            .WithDescription("Lấy danh sách tất cả các sân thuộc các trung tâm mà người dùng hiện tại sở hữu");
            // Update Court
            group.MapPut("/{id:guid}", [Authorize(Policy = "CourtOwnerOfCenter")] async (Guid id, [FromBody] UpdateCourtRequest request, ISender sender) =>
            {
                var command = new UpdateCourtCommand(id, request.Court);
                var result = await sender.Send(command);
                var response = new UpdateCourtResponse(result.IsSuccess);
                return Results.Ok(response);
            })
            .WithName("UpdateCourt")
            .Produces<UpdateCourtResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithSummary("Cập nhật sân")
            .WithDescription("Cập nhật thông tin sân (yêu cầu quyền sở hữu)");

            // Delete Court
            group.MapDelete("/{id:guid}", [Authorize(Policy = "CourtOwnerOfCenter")] async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteCourtCommand(id));
                var response = new DeleteCourtResponse(result.IsSuccess);
                return Results.Ok(response);
            })
            .WithName("DeleteCourt")
            .Produces<DeleteCourtResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Xóa sân")
            .WithDescription("Xóa một sân cụ thể theo ID (yêu cầu quyền sở hữu)");

            // Get Court Availability
            group.MapGet("/{id:guid}/availability", async (
                Guid id,
                [FromQuery] DateTime startDate,
                [FromQuery] DateTime endDate,
                ISender sender) =>
            {
                var query = new GetCourtAvailabilityQuery(id, startDate, endDate);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetCourtAvailability")
            .Produces<GetCourtAvailabilityResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Lấy lịch khả dụng của sân")
            .WithDescription("Lấy thông tin về thời gian khả dụng của sân trong khoảng thời gian từ startDate đến endDate, bao gồm trạng thái đặt sân");
        }
    }
}