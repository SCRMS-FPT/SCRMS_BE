using CourtBooking.Application.SportManagement.Commands.CreateSport;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Endpoints;

public record CreateSportRequest(string Name, string Description, string Icon);
public record CreateSportResponse(Guid Id);

public class CreateSport : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/sports", async ([FromBody] CreateSportRequest request, ISender sender) =>
        {
            var command = new CreateSportCommand(request.Name, request.Description, request.Icon);
            var result = await sender.Send(command);
            var response = new CreateSportResponse(result.Id);
            return Results.Created($"/api/sports/{response.Id}", response);
        })
        .WithName("CreateSport")
        .Produces<CreateSportResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Create Sport")
        .WithDescription("Create a new sport");
    }
}
