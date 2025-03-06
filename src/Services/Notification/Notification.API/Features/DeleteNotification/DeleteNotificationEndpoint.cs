using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Notification.API.Features.DeleteNotification
{
    public class DeleteNotificationEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/notifications/{notificationId:guid}",
                async (
                [FromServices] ISender sender,
                HttpContext httpContext,
                Guid notificationId) =>
                {
                    var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                        ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                        return Results.Unauthorized();

                    var command = new DeleteNotificationCommand(notificationId, userId);


                    var result = await sender.Send(command);

                    return Results.Ok();
                })
            .RequireAuthorization("Admin")
            .WithName("DeleteNotification")
            .Produces<DeleteNotificationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Delete a notification")
            .WithDescription("Deletes a specific notification for the user");

        }
    }
}
