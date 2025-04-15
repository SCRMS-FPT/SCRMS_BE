using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Notification.API.Features.ReadAllNotification
{
    public class ReadAllNotificationsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/notifications/read-all", async (
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                    ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new ReadAllNotificationCommand(userId);
                await sender.Send(command);

                return Results.Ok(new { message = "All notifications marked as read." });
            })
            .WithName("ReadAllNotifications")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Mark all notifications as read")
            .WithDescription("Marks all unread notifications as read for the currently authenticated user.");
        }
    }

}
