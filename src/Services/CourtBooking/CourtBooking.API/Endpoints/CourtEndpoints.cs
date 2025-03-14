using BuildingBlocks.Pagination;
using CourtBooking.Application.CourtManagement.Command.CreateCourt;
using CourtBooking.Application.CourtManagement.Command.UpdateCourt;
using CourtBooking.Application.CourtManagement.Command.DeleteCourt;
using CourtBooking.Application.CourtManagement.Queries.GetCourts;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Endpoints
{
    public record CreateCourtRequest(CourtCreateDTO Court);
    public record CreateCourtResponse(Guid Id);
    public record GetCourtDetailsResponse(CourtDTO Court);
    public record GetCourtsResponse(PaginatedResult<CourtDTO> Courts);
    public record GetAllCourtsOfSportCenterResponse(List<CourtDTO> Courts);
    public record UpdateCourtRequest(CourtUpdateDTO Court);
    public record UpdateCourtResponse(bool IsSuccess);
    public record DeleteCourtResponse(bool IsSuccess);

    public class CourtEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/courts").WithTags("Court");

            // Create Court
            group.MapPost("/", async ([FromBody] CreateCourtRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateCourtCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<CreateCourtResponse>();
                return Results.Created($"/api/courts/{response.Id}", response);
            })
            .WithName("CreateCourt")
            .Produces<CreateCourtResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Court")
            .WithDescription("Create a new court");

            // Get Court Details
            group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetCourtDetailsQuery(id));
                var response = result.Adapt<GetCourtDetailsResponse>();
                return Results.Ok(response);
            })
            .WithName("GetCourtDetails")
            .Produces<GetCourtDetailsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Court Details")
            .WithDescription("Get details of a specific court by ID");

            // Get Courts
            group.MapGet("/", async ([AsParameters] PaginationRequest request, ISender sender) =>
            {
                var result = await sender.Send(new GetCourtsQuery(request));
                var response = result.Adapt<GetCourtsResponse>();
                return Results.Ok(response);
            })
            .WithName("GetCourts")
            .Produces<GetCourtsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Courts")
            .WithDescription("Get a paginated list of courts");

            // Update Court
            group.MapPut("/", async ([FromBody] UpdateCourtRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateCourtCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<UpdateCourtResponse>();
                return Results.Ok(response);
            })
            .WithName("UpdateCourt")
            .Produces<UpdateCourtResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Update Court")
            .WithDescription("Update an existing court");

            // Delete Court
            group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteCourtCommand(id));
                var response = result.Adapt<DeleteCourtResponse>();
                return Results.Ok(response);
            })
            .WithName("DeleteCourt")
            .Produces<DeleteCourtResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete Court")
            .WithDescription("Delete a specific court by ID");

            // Get All Courts of Sport Center
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
            .WithDescription("Get all courts of a specific sport center by ID");
        }
    }
}