
using CourtBooking.Application.CourtManagement.Command.CreateCourt;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Endpoints
{
    public record CreateCourtRequest(CourtCreateDTO Court);
    public record CreateCourtResponse(Guid Id);
    public class CreateCourt : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/courts", async ([FromBody] CreateCourtRequest request, ISender sender) =>
            {
            var command = request.Adapt<CreateCourtCommand>();
            var result = await sender.Send(command);
            var response = result.Adapt<CreateCourtResponse>();
            return Results.Created($"/api/courts/{response.Id}", response);
            })
            .WithName("CreateCourt")
            .Produces<CreateCourtResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Court")
            .WithDescription("Create a new court");
        }
    }
}
