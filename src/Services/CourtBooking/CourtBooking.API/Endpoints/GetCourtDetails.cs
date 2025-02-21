using BuildingBlocks.Pagination;
using CourtBooking.Application.CourtManagement.Queries.GetCourts;
using CourtBooking.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Endpoints
{
    public record GetCourtDetailsResponse(CourtDTO Court);

    public class GetCourtDetails : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/courts/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetCourtDetailsQuery(id));
                var response = result.Adapt<GetCourtDetailsResponse>();
                return Results.Ok(response);
            })
            .WithName("GetCourtDetails")
            .Produces<GetCourtDetailsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Court Details")
            .WithDescription("Get details of a specific court by ID");
        }
    }
}
