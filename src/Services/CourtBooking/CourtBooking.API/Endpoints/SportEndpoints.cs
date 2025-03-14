using CourtBooking.Application.SportManagement.Commands.CreateSport;
using CourtBooking.Application.SportManagement.Commands.UpdateSport;
using CourtBooking.Application.SportManagement.Commands.DeleteSport;
using CourtBooking.Application.SportManagement.Queries.GetSports;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

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
            group.MapPost("/", async ([FromBody] CreateSportRequest request, ISender sender) =>
            {
                var command = new CreateSportCommand(request.Name, request.Description, request.Icon);
                var result = await sender.Send(command);
                var response = new CreateSportResponse(result.Id);
                return Results.Created($"/api/sports/{response.Id}", response);
            })
            .WithName("CreateSport")
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

            // Update Sport
            group.MapPut("/", async ([FromBody] UpdateSportRequest request, ISender sender) =>
            {
                var command = new UpdateSportCommand(request.Id, request.Name, request.Description, request.Icon);
                var result = await sender.Send(command);
                var response = new UpdateSportResponse(result.IsSuccess);
                return Results.Ok(response);
            })
            .WithName("UpdateSport")
            .Produces<UpdateSportResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Update Sport")
            .WithDescription("Update an existing sport");

            // Delete Sport
            group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
            {
                var command = new DeleteSportCommand(id);
                var result = await sender.Send(command);
                var response = new DeleteSportResponse(result.IsSuccess, result.Message);
                return result.IsSuccess ? Results.Ok(response) : Results.BadRequest(response);
            })
            .WithName("DeleteSport")
            .Produces<DeleteSportResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Delete Sport")
            .WithDescription("Delete an existing sport if it is not associated with any court");
        }
    }
}