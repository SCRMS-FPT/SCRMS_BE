﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

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
            app.MapPost("/packages", async (
                [FromBody] CreatePackageRequest request,
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                        ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var command = new CreatePackageCommand(
                    coachUserId,
                    request.Name,
                    request.Description,
                    request.Price,
                    request.SessionCount
                );
                var result = await sender.Send(command);
                return Results.Created($"/packages/{result.Id}", result);
            })
            .RequireAuthorization("Coach") // Yêu cầu xác thực và role Coach
            .WithName("CreatePackage")
            .Produces(StatusCodes.Status201Created);
        }
    }
}