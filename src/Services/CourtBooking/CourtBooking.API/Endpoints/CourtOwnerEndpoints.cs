using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Carter;
using CourtBooking.Application.CourtOwnerManagement.Queries.GetDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CourtBooking.API.Endpoints
{
    public class CourtOwnerEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/courtowner").WithTags("CourtOwner");

            // Get Dashboard
            group.MapGet("/dashboard", async (HttpContext httpContext, ISender sender) =>
            {
                // Extract user ID from JWT token
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                             ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var ownerId))
                {
                    return Results.Unauthorized();
                }

                var query = new GetCourtOwnerDashboardQuery(ownerId);
                var result = await sender.Send(query);

                return Results.Ok(result);
            })
            .RequireAuthorization(policy => policy.RequireRole("CourtOwner"))
            .WithName("GetCourtOwnerDashboard")
            .Produces<GetCourtOwnerDashboardResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get Court Owner Dashboard")
            .WithDescription("Returns statistics and metrics for court owner's business including sport centers, courts, bookings and revenue");
        }
    }
}