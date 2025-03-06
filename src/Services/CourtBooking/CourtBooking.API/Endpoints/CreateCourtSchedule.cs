using CourtBooking.Application.CourtManagement.Command.CreateCourtSchedule;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Endpoints
{
    public record CreateCourtScheduleRequest(CourtScheduleDTO CourtSchedule);
    public record CreateCourtScheduleResponse(Guid Id);

    public class CreateCourtSchedule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/courtschedules", async ([FromBody] CreateCourtScheduleRequest request, ISender sender) =>
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
        }
    }
}
