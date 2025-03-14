using CourtBooking.Application.CourtManagement.Command.CreateCourtSchedule;
using CourtBooking.Application.CourtManagement.Command.UpdateCourtSchedule;
using CourtBooking.Application.CourtManagement.Command.DeleteCourtSchedule;
using CourtBooking.Application.CourtManagement.Queries.GetCourtSchedulesByCourtId;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

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
            group.MapPost("/", async ([FromBody] CreateCourtScheduleRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateCourtScheduleCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<CreateCourtScheduleResponse>();
                return Results.Created($"/api/courtschedules/{response.Id}", response);
            })
            .WithName("CreateCourtSchedule")
            .Produces<CreateCourtScheduleResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Court Schedule")
            .WithDescription("Create a new court schedule");

            // Get Court Schedules By Court Id
            app.MapGet("/api/courts/{courtId}/schedules", async (Guid courtId, ISender sender) =>
            {
                var query = new GetCourtSchedulesByCourtIdQuery(courtId);
                var result = await sender.Send(query);
                var response = new GetCourtSchedulesByCourtIdResponse(result.CourtSchedules);
                return Results.Ok(response);
            })
            .WithName("GetCourtSchedulesByCourtId")
            .Produces<GetCourtSchedulesByCourtIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Court Schedules")
            .WithDescription("Get all schedules for a specific court by court ID");

            // Update Court Schedule
            group.MapPut("/", async ([FromBody] UpdateCourtScheduleRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateCourtScheduleCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<UpdateCourtScheduleResponse>();
                return Results.Ok(response);
            })
            .WithName("UpdateCourtSchedule")
            .Produces<UpdateCourtScheduleResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Update Court Schedule")
            .WithDescription("Update an existing court schedule");

            // Delete Court Schedule
            group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
            {
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