
using CourtBooking.Application.CourtManagement.Command.DeleteCourt;

namespace CourtBooking.API.Endpoints
{
    public record DeleteCourtRespone(bool IsSuccess);
    public class DeleteCourt : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/courts/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteCourtCommand(id));
                var response = result.Adapt<DeleteCourtRespone>();
                return Results.Ok(response);
            }).WithName("DeleteCourt")
            .Produces<DeleteCourtRespone>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete Court")
            .WithDescription("Delete a specific court by ID");
        }
    }
}
