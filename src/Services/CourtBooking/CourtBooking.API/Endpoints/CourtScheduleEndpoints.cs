using CourtBooking.Application.CourtManagement.Command.CreateCourtSchedule;
using CourtBooking.Application.CourtManagement.Command.UpdateCourtSchedule;
using CourtBooking.Application.CourtManagement.Command.DeleteCourtSchedule;
using CourtBooking.Application.CourtManagement.Queries.GetCourtSchedulesByCourtId;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;  // Đảm bảo có using cho repository

namespace CourtBooking.API.Endpoints
{
    public record CreateCourtScheduleRequest(CourtScheduleDTO CourtSchedule);
    public record CreateCourtScheduleResponse(Guid Id);
    public record GetCourtSchedulesByCourtIdResponse(List<CourtScheduleDTO> CourtSchedules);
    public record UpdateCourtScheduleRequest(CourtScheduleUpdateDTO CourtSchedule);
    public record UpdateCourtScheduleResponse(bool IsSuccess);
    public record DeleteCourtScheduleResponse(bool IsSuccess);

    public class CourtScheduleEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/courtschedules").WithTags("CourtSchedule");

            // Create Court Schedule
            group.MapPost("/", async (
                [FromBody] CreateCourtScheduleCommand command,
                HttpContext httpContext,
                ISender sender,
                ICourtRepository courtRepository) =>
            {
                // Lấy thông tin người dùng từ JWT
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);
                if (userIdClaim == null || roleClaim == null)
                    return Results.Unauthorized();

                var userId = Guid.Parse(userIdClaim.Value);
                if (roleClaim.Value != "CourtOwner")
                    return Results.Forbid();

                // Kiểm tra xem người dùng có phải chủ của sàn chứa court không
                if (!await courtRepository.IsOwnedByUserAsync(command.CourtId, userId, httpContext.RequestAborted))
                    return Results.Forbid();

                var result = await sender.Send(command);
                return Results.Created($"/api/court-schedules/{result.Id}", result);
            })
            .WithName("CreateCourtSchedule")
            .Produces<CreateCourtScheduleResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Court Schedule")
            .WithDescription("Create a new court schedule");

            // Get Court Schedules By Court Id
            group.MapGet("/{courtId:guid}/schedules", async (
                Guid courtId,
                [FromQuery] int? day,
                ISender sender) =>
            {
                var query = new GetCourtSchedulesByCourtIdQuery(courtId, day);
                var result = await sender.Send(query);
                return Results.Ok(result.CourtSchedules);
            })
            .WithName("GetCourtSchedulesByCourtId")
            .Produces<List<CourtScheduleDTO>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Court Schedules")
            .WithDescription("Get all schedules for a specific court by court ID, optionally filtered by day");

            // Update Court Schedule
            group.MapPut("/", async (
                [FromBody] UpdateCourtScheduleRequest request,
                HttpContext httpContext,
                ISender sender,
                ICourtRepository courtRepository,
                ICourtScheduleRepository courtScheduleRepository) =>
            {
                // Lấy thông tin người dùng từ JWT
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);
                if (userIdClaim == null || roleClaim == null)
                    return Results.Unauthorized();

                var userId = Guid.Parse(userIdClaim.Value);
                if (roleClaim.Value != "CourtOwner")
                    return Results.Forbid();

                // Lấy thông tin lịch hiện có để biết được CourtId
                var existingSchedule = await courtScheduleRepository.GetCourtScheduleByIdAsync(
                    CourtScheduleId.Of(request.CourtSchedule.Id),
                    httpContext.RequestAborted);
                if (existingSchedule == null)
                    return Results.NotFound("Court schedule not found");

                // Kiểm tra quyền sở hữu dựa trên CourtId của lịch
                if (!await courtRepository.IsOwnedByUserAsync(existingSchedule.CourtId.Value, userId, httpContext.RequestAborted))
                    return Results.Forbid();

                var command = request.Adapt<UpdateCourtScheduleCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<UpdateCourtScheduleResponse>();
                return Results.Ok(response);
            })
            .WithName("UpdateCourtSchedule").RequireAuthorization("CourtOwner")
            .Produces<UpdateCourtScheduleResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Update Court Schedule")
            .WithDescription("Update an existing court schedule");

            // Delete Court Schedule
            group.MapDelete("/{id:guid}", async (
                Guid id,
                HttpContext httpContext,
                ISender sender,
                ICourtRepository courtRepository,
                ICourtScheduleRepository courtScheduleRepository) =>
            {
                // Lấy thông tin người dùng từ JWT
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);
                if (userIdClaim == null || roleClaim == null)
                    return Results.Unauthorized();

                var userId = Guid.Parse(userIdClaim.Value);
                if (roleClaim.Value != "CourtOwner")
                    return Results.Forbid();

                // Lấy lịch cần xóa để biết được CourtId (sử dụng CourtScheduleId.Of(id))
                var schedule = await courtScheduleRepository.GetCourtScheduleByIdAsync(CourtScheduleId.Of(id), httpContext.RequestAborted);
                if (schedule == null)
                    return Results.NotFound("Court schedule not found");

                // Kiểm tra quyền sở hữu dựa trên CourtId của lịch
                if (!await courtRepository.IsOwnedByUserAsync(schedule.CourtId.Value, userId, httpContext.RequestAborted))
                    return Results.Forbid();

                var command = new DeleteCourtScheduleCommand(id);
                var result = await sender.Send(command);
                var response = new DeleteCourtScheduleResponse(result.IsSuccess);
                return Results.Ok(response);
            })
            .WithName("DeleteCourtSchedule")
            .Produces<DeleteCourtScheduleResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete Court Schedule")
            .WithDescription("Delete an existing court schedule");
        }
    }
}