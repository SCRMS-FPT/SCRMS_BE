using CourtBooking.Application.SportManagement.Commands.UpdateSport;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Endpoints;

public record UpdateSportRequest(Guid Id, string Name, string Description, string Icon);
public record UpdateSportResponse(bool IsSuccess);

public class UpdateSport : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/sports", async ([FromBody] UpdateSportRequest request, ISender sender) =>
        {
            var command = new UpdateSportCommand(request.Id, request.Name, request.Description, request.Icon);
            var result = await sender.Send(command);
            var response = new UpdateSportResponse(result.IsSuccess);
            return Results.Ok(response);
        })
        .WithName("UpdateSport")
        .Produces<UpdateSportResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Update Sport")
        .WithDescription("Update an existing sport");
    }
}
