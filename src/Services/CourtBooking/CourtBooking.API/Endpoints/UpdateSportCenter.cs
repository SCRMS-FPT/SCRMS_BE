using CourtBooking.Application.DTOs;
using CourtBooking.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

public record UpdateSportCenterRequest(
    Guid SportCenterId,
    string Name,
    string PhoneNumber,
    string Description,
    LocationDTO Location,
    GeoLocation LocationPoint,
    SportCenterImages Images
);

public record UpdateSportCenterResponse(bool Success);

public class UpdateSportCenterEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/sportcenter", async ([FromBody] UpdateSportCenterRequest request, ISender sender) =>
        {
            var command = request.Adapt<UpdateSportCenterCommand>();
            var result = await sender.Send(command);
            var response = result.Adapt<UpdateSportCenterResponse>();
            return Results.Ok(response);
        })
        .WithName("UpdateSportCenter")
        .Produces<UpdateSportCenterResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Update Sport Center")
        .WithDescription("Updates basic Sport Center info including location and images, without modifying courts.");
    }
}
