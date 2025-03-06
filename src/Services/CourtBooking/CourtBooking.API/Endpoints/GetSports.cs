using CourtBooking.Application.DTOs;
using CourtBooking.Application.SportManagement.Queries.GetSports;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Endpoints;

public record GetSportsResponse(List<SportDTO> Sports);

public class GetSports : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/sports", async (ISender sender) =>
        {
            var query = new GetSportsQuery();
            var result = await sender.Send(query);
            var response = new GetSportsResponse(result.Sports);
            return Results.Ok(response);
        })
        .WithName("GetSports")
        .Produces<GetSportsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Sports")
        .WithDescription("Retrieve all sports");
    }
}
