using Coach.API.Data.Models;
using Coach.API.Features.Coaches.GetCoaches;
using Microsoft.AspNetCore.Mvc;

namespace Coach.API.Features.Coaches.GetCoachById
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
            .WithName("GetCoachById")
            .Produces<CoachResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound).WithTags("Coach");
        }
    }
}