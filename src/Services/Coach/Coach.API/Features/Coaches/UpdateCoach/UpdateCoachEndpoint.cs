using Coach.API.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Features.Coaches.UpdateCoach
{
    public record UpdateCoachRequest(string Bio, decimal RatePerHour, List<Guid> ListSport);

    public class UpdateCoachEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)

        {
            app.MapPut("/api/coaches/{coachId:guid}", async (
           [FromBody] UpdateCoachRequest request,
           [FromQuery] Guid coachId,
            [FromServices] ISender sender,
            HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new UpdateCoachCommand(coachId, request.Bio, request.RatePerHour, request.ListSport);

                var result = await sender.Send(command);
                return Results.Ok();
            })
            .RequireAuthorization("Admin")
            .WithName("UpdateCoach")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Update Coach")
            .WithDescription("Update coach profile");
        }
    }
}