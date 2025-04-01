using Microsoft.AspNetCore.Mvc;

namespace Coach.API.Features.Packages.GetActivePackages
{
    public class GetActivePackagesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/coaches/{coachId:guid}/active-packages", async (
                Guid coachId,
                [FromServices] ISender sender) =>
            {
                var query = new GetActivePackagesQuery(coachId);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetActivePackages")
            .Produces<List<PackageResponse>>(StatusCodes.Status200OK)
            .WithSummary("Get Active Coach Packages")
            .WithDescription("Retrieve all active packages offered by a specific coach.").WithTags("Package");
        }
    }
}