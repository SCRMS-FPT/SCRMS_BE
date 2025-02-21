using BuildingBlocks.Pagination;
using CourtBooking.Application.CourtManagement.Queries.GetCourts;
using CourtBooking.Application.DTOs;

namespace CourtBooking.API.Endpoints
{
    public record GetCourtsRespone(PaginatedResult<CourtDTO> courts);
    public class GetCourts : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/courts",async ([AsParameters] PaginationRequest request, ISender sender) =>
            {
                var result = await sender.Send(new GetCourtsQuery(request));
                var response = result.Adapt<GetCourtsRespone>();
                return Results.Ok(response);
            }).WithName("GetCourts")
            .Produces<GetCourtsRespone>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Courts")
            .WithDescription("Get Courts");
        }
    }
}
