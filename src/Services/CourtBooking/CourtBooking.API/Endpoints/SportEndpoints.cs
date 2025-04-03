using CourtBooking.Application.SportManagement.Commands.CreateSport;
using CourtBooking.Application.SportManagement.Commands.UpdateSport;
using CourtBooking.Application.SportManagement.Commands.DeleteSport;
using CourtBooking.Application.SportManagement.Queries.GetSports;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using CourtBooking.Application.SportManagement.Queries.GetSportById;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CourtBooking.API.Endpoints
{
    public record CreateSportRequest(string Name, string Description, string Icon);
    public record CreateSportResponse(Guid Id);
    public record GetSportsResponse(List<SportDTO> Sports);
    public record UpdateSportRequest(Guid Id, string Name, string Description, string Icon);
    public record UpdateSportResponse(bool IsSuccess);
    public record DeleteSportResponse(bool IsSuccess, string Message);

    public class SportEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/sports").WithTags("Sport");

            // Create Sport
            group.MapPost("/", async ([FromBody] CreateSportRequest request, HttpContext httpContext, ISender sender) =>
            {
                var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);
                if (roleClaim == null || roleClaim.Value != "Admin")
                    return Results.Forbid();
                var command = new CreateSportCommand(request.Name, request.Description, request.Icon);
                var result = await sender.Send(command);
                var response = new CreateSportResponse(result.Id);
                return Results.Created($"/api/sports/{response.Id}", response);
            })
            .WithName("CreateSport")
            .RequireAuthorization()
            .Produces<CreateSportResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Sport")
            .WithDescription("Create a new sport");

            // Get Sports
            group.MapGet("/", async (ISender sender) =>
            {
                var query = new GetSportsQuery();
                var result = await sender.Send(query);
                var response = new GetSportsResponse(result.Sports);
                return Results.Ok(response);
            })
            .WithName("GetSports")
            .Produces<GetSportsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Sports")
            .WithDescription("Retrieve all sports");

            group.MapGet("/{sportId:guid}", async (
                Guid sportId,
                [FromServices] ISender sender) =>
            {
                var query = new GetSportByIdQuery(sportId);
                var result = await sender.Send(query);
                return result == null ? Results.NotFound() : Results.Ok(result);
            })
            .WithName("GetSportById")
            .Produces<SportDTO>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Lấy thông tin chi tiết môn thể thao")
            .WithDescription("Trả về thông tin chi tiết của một môn thể thao dựa trên sportId.");

            // Update Sport
            group.MapPut("/", async ([FromBody] UpdateSportRequest request, HttpContext httpContext, ISender sender) =>
            {
                var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);
                if (roleClaim == null || roleClaim.Value != "Admin")
                    return Results.Forbid();

                var command = new UpdateSportCommand(request.Id, request.Name, request.Description, request.Icon);
                var result = await sender.Send(command);
                var response = new UpdateSportResponse(result.IsSuccess);
                return Results.Ok(response);
            })
            .WithName("UpdateSport")
            .RequireAuthorization()
            .Produces<UpdateSportResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Update Sport")
            .WithDescription("Update an existing sport");

            // Delete Sport
            group.MapDelete("/{id:guid}", async (Guid id, HttpContext httpContext, ISender sender) =>
            {
                var roleClaim = httpContext.User.FindFirst(ClaimTypes.Role);
                if (roleClaim == null || roleClaim.Value != "Admin")
                    return Results.Forbid();
                var command = new DeleteSportCommand(id);
                var result = await sender.Send(command);
                var response = new DeleteSportResponse(result.IsSuccess, result.Message);
                return result.IsSuccess ? Results.Ok(response) : Results.BadRequest(response);
            })
            .WithName("DeleteSport")
            .RequireAuthorization()
            .Produces<DeleteSportResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Delete Sport")
            .WithDescription("Delete an existing sport if it is not associated with any court");
        }
    }
}