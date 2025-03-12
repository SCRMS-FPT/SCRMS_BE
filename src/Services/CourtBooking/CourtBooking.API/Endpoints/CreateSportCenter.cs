﻿using CourtBooking.Application.SportCenterManagement.Commands.CreateSportCenter;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Endpoints
{
    public record CreateSportCenterRequest(CreateSportCenterCommand SportCenter);
    public record CreateSportCenterResponse(Guid Id);
    public class CreateSportCenter : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/sportcenters", async ([FromBody] CreateSportCenterRequest request, ISender sender) =>
            {
                var command = request.SportCenter.Adapt<CreateSportCenterCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<CreateSportCenterResponse>();
                return Results.Created($"/api/sportcenters/{response.Id}", response);
            })
            .WithName("CreateSportCenter")
            .WithGroupName("SportCenter")
            .Produces<CreateSportCenterResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Sport Center")
            .WithDescription("Create a new sport center");
        }
    }
}
