using CourtBooking.Application.SportManagement.Commands.DeleteSport;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Endpoints;

public record DeleteSportRequest(Guid SportId);
public record DeleteSportResponse(bool IsSuccess, string Message);

public class DeleteSport : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/sports/{id:guid}", async (Guid id, ISender sender) =>
        {
            var command = new DeleteSportCommand(id);
            var result = await sender.Send(command);
            var response = new DeleteSportResponse(result.IsSuccess, result.Message);
            return result.IsSuccess ? Results.Ok(response) : Results.BadRequest(response);
        })
        .WithName("DeleteSport")
        .Produces<DeleteSportResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Delete Sport")
        .WithDescription("Delete an existing sport if it is not associated with any court");
    }
}
