using BuildingBlocks.Pagination;
using CourtBooking.Application.SportCenterManagement.Commands.CreateSportCenter;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using CourtBooking.Application.CourtManagement.Queries.GetSportCenters;
using CourtBooking.Application.CourtManagement.Queries.GetSportCenterById;
using CourtBooking.Application.CourtManagement.Command.UpdateSportCenter;
using Microsoft.AspNetCore.Authorization;
using CourtBooking.Application.CourtManagement.Queries.GetAllCourtsOfSportCenter;

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
            group.MapPost("/", async ([FromBody] CreateSportCenterCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                var response = new CreateSportCenterResponse(result.Id);
                return Results.Created($"/api/sportcenters/{response.Id}", response);
            })
            .WithName("CreateSportCenter")
            .RequireAuthorization("AdminOrCourtOwner") // Giới hạn quyền Admin hoặc CourtOwner
            .Produces<CreateSportCenterResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Sport Center")
            .WithDescription("Create a new sport center");

            // Get Sport Centers
            group.MapGet("/", async (
                [FromQuery] int page, // 1-based
                [FromQuery] int limit,
                [FromQuery] string? city,
                [FromQuery] string? name,
                ISender sender) =>
            {
                var paginationRequest = new PaginationRequest(page - 1, limit); // Chuyển page 1-based sang 0-based
                var query = new GetSportCentersQuery(paginationRequest, city, name);
                var result = await sender.Send(query);
                var response = result.Adapt<GetSportCentersResponse>();
                return Results.Ok(response);
            })
            .WithName("GetSportCenters")
            .Produces<GetSportCentersResponse>(StatusCodes.Status200OK)
            .WithSummary("Get Sport Centers")
            .WithDescription("Get a paginated list of sport centers with optional filters");

            // Get All Courts of Sport Center
            group.MapGet("/{id:guid}/courts", [Authorize] async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetAllCourtsOfSportCenterQuery(id));
                var response = new GetAllCourtsOfSportCenterResponse(result.Courts);
                return Results.Ok(response);
            })
            .WithName("GetAllCourtsOfSportCenter")
            .Produces<GetAllCourtsOfSportCenterResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Lấy tất cả sân của một trung tâm")
            .WithDescription("Lấy tất cả sân của một trung tâm thể thao cụ thể theo ID");

            // Update Sport Center
            group.MapPut("/{centerId:guid}", async (Guid centerId, [FromBody] UpdateSportCenterCommand command, ISender sender) =>
            {
                command = command with { SportCenterId = centerId }; // Gán centerId từ path vào command
                var result = await sender.Send(command);
                return Results.Ok(result.SportCenter);
            })
            .WithName("UpdateSportCenter")
            .RequireAuthorization("AdminOrCourtOwner") // Yêu cầu JWT và quyền Admin hoặc CourtOwner
            .Produces<SportCenterListDTO>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update Sport Center")
            .WithDescription("Updates the information of an existing sport center");

            group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
            {
                var query = new GetSportCenterByIdQuery(id);
                var result = await sender.Send(query);
                return Results.Ok(result.SportCenter);
            })
            .WithName("GetSportCenterById")
            .RequireAuthorization() // Yêu cầu JWT, cho phép mọi user đã đăng nhập
            .Produces<SportCenterListDTO>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Sport Center By ID")
            .WithDescription("Get detailed information of a specific sport center");
        }
    }
}