using Identity.Application.Identity.Commands.ServicePackagesManagement;
using Identity.Application.Identity.Queries.ServicePackagesManagement;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Endpoints
{
    public class ServicePackages : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/service-packages").RequireAuthorization("Admin");

            group.MapGet("/", async (ISender sender) =>
            {
                var result = await sender.Send(new GetServicePackagesQuery());
                return Results.Ok(result);
            });

            group.MapGet("/{id}", async (int id, ISender sender) =>
            {
                var result = await sender.Send(new GetServicePackageByIdQuery(id));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapPost("/", async ([FromBody] CreateServicePackageRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateServicePackageCommand>();
                var result = await sender.Send(command);
                return Results.Created($"/api/service-packages/{result.Id}", result);
            });

            group.MapPut("/{id}", async (int id, [FromBody] UpdateServicePackageRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateServicePackageCommand>() with { Id = id };
                var result = await sender.Send(command);
                return Results.Ok(result);
            });

            group.MapDelete("/{id}", async (int id, ISender sender) =>
            {
                await sender.Send(new DeleteServicePackageCommand(id));
                return Results.NoContent();
            });
        }
    }

    public record CreateServicePackageRequest(
        string Name,
        string Description,
        decimal Price,
        int DurationDays
    );

    public record UpdateServicePackageRequest(
        string Name,
        string Description,
        decimal Price,
        int DurationDays
    );
}
