using BuildingBlocks.Pagination;
using CourtBooking.Application.CourtManagement.Queries.GetSportCenters;
using CourtBooking.Application.DTOs;

namespace CourtBooking.API.Endpoints
{
    public record GetSportCentersResponse(PaginatedResult<SportCenterListDTO> SportCenters);

    public class GetSportCenters : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/sportcenters", async ([AsParameters] PaginationRequest request, ISender sender) =>
            {
                var result = await sender.Send(new GetSportCentersQuery(request));
                var response = result.Adapt<GetSportCentersResponse>();
                return Results.Ok(response);
            })
            .WithName("GetSportCenters")
            .WithGroupName("SportCenter")
            .Produces<GetSportCentersResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Sport Centers")
            .WithDescription("Get a paginated list of sport centers.");
        }
    }
}
