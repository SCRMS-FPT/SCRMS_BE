using CourtBooking.Application.CourtManagement.Command.DeleteCourtSchedule;

namespace CourtBooking.API.Endpoints;

public record DeleteCourtScheduleRequest(Guid CourtScheduleId);
public record DeleteCourtScheduleResponse(bool IsSuccess);

public class DeleteCourtSchedule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/courtschedules/{id:guid}", async (Guid id, ISender sender) =>
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
