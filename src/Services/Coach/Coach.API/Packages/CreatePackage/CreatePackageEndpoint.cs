using Microsoft.AspNetCore.Mvc;

namespace Coach.API.Packages.CreatePackage
{
    public record CreatePackageRequest(
    Guid CoachId,
    string Name,
    string Description,
    decimal Price,
    int SessionCount);

    public class CreatePackageEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/packages", async ([FromBody] CreatePackageRequest request, [FromServices] ISender sender) =>
            {
                var command = request.Adapt<CreatePackageCommand>();
                var result = await sender.Send(command);
                return Results.Created($"/packages/{result.Id}", result);
            });
        }
    }
}