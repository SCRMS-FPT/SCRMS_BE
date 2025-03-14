using BuildingBlocks.Pagination;
using CourtBooking.Application.SportCenterManagement.Commands.CreateSportCenter;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using CourtBooking.Application.CourtManagement.Queries.GetSportCenters;

namespace CourtBooking.API.Endpoints
{
    public record CreateSportCenterRequest(CreateSportCenterCommand SportCenter);
    public record CreateSportCenterResponse(Guid Id);
    public record GetSportCentersResponse(PaginatedResult<SportCenterDTO> SportCenters);
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

    public class SportCenterEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/sportcenters").WithTags("SportCenter");

            // Create Sport Center
            group.MapPost("/", async ([FromBody] CreateSportCenterRequest request, ISender sender) =>
            {
                var command = request.SportCenter.Adapt<CreateSportCenterCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<CreateSportCenterResponse>();
                return Results.Created($"/api/sportcenters/{response.Id}", response);
            })
            .WithName("CreateSportCenter")
            .Produces<CreateSportCenterResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Sport Center")
            .WithDescription("Create a new sport center");

            // Get Sport Centers
            group.MapGet("/", async ([AsParameters] PaginationRequest request, ISender sender) =>
            {
                var result = await sender.Send(new GetSportCentersQuery(request));
                var response = result.Adapt<GetSportCentersResponse>();
                return Results.Ok(response);
            })
            .WithName("GetSportCenters")
            .Produces<GetSportCentersResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Sport Centers")
            .WithDescription("Get a paginated list of sport centers");

            // Update Sport Center
            group.MapPut("/", async ([FromBody] UpdateSportCenterRequest request, ISender sender) =>
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
            .WithDescription("Updates basic sport center info including location and images");
        }
    }
}