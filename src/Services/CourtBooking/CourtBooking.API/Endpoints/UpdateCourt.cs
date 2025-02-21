using CourtBooking.Application.CourtManagement.Command.UpdateCourt;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Endpoints;

public record UpdateCourtRequest(CourtUpdateDTO Court);
public record UpdateCourtResponse(bool IsSuccess);

public class UpdateCourt : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/courts", async ([FromBody] UpdateCourtRequest request, ISender sender) =>
        {
            var command = request.Adapt<UpdateCourtCommand>();
            var result = await sender.Send(command);
            var response = result.Adapt<UpdateCourtResponse>();
            return Results.Ok(response);
        })
        .WithName("UpdateCourt")
        .Produces<UpdateCourtResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Update Court")
        .WithDescription("Update an existing court");
    }
}
