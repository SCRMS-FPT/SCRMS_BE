using CourtBooking.Application.CourtManagement.Command.UpdateCourtSchedule;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Endpoints
{
    public record UpdateCourtScheduleRequest(CourtScheduleUpdateDTO CourtSchedule);
    public record UpdateCourtScheduleResponse(bool IsSuccess);

    public class UpdateCourtSchedule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/courtschedules", async ([FromBody] UpdateCourtScheduleRequest request, ISender sender) =>
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
        }
    }
}
