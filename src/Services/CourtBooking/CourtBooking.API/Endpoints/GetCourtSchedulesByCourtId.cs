using CourtBooking.Application.CourtManagement.Queries.GetCourtSchedulesByCourtId;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Endpoints;

public record GetCourtSchedulesByCourtIdRequest(Guid CourtId);
public record GetCourtSchedulesByCourtIdResponse(List<CourtScheduleDTO> CourtSchedules);

public class GetCourtSchedulesByCourtId : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
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
    }
}
