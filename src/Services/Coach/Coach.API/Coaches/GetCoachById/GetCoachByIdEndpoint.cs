using Coach.API.Coaches.GetCoaches;
using Microsoft.AspNetCore.Mvc;

namespace Coach.API.Coaches.GetCoachById
{
    public class GetCoachByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/coaches/{id}", async (Guid id, [FromServices] ISender sender) =>
            {
                var result = await sender.Send(new GetCoachByIdQuery(id));
                return Results.Ok(result);
            })
                .RequireAuthorization("Admin")
            .WithName("GetCoachById")
            .Produces<CoachResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}