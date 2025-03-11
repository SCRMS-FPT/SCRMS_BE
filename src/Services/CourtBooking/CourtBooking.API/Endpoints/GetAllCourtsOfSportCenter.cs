
using CourtBooking.Application.DTOs;

namespace CourtBooking.API.Endpoints
{
    public record GetAllCourtsOfSportCenterResponse(List<CourtDTO> Courts);

    public class GetAllCourtsOfSportCenter : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/sportcenter/{id:guid}/courts", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetAllCourtsOfSportCenterQuery(id));
                var response = result.Adapt<GetAllCourtsOfSportCenterResponse>();
                return Results.Ok(response);
            })
            .WithName("GetAllCourtsOfSportCenter")
            .Produces<GetAllCourtsOfSportCenterResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get All Courts Of A Sport Center")
            .WithDescription("Get details of a specific court by ID");
        }
    }
}
