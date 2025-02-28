using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Notification.API.Features.GetNotifications
{
    public record GetNotificationsRequest(Guid UserId, int Page, int Limit, Boolean? IsRead);
    public class GetNotificationsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/notifications",
                async ([FromBody] 
                GetNotificationsRequest request,
                [FromServices] ISender sender, 
                HttpContext httpContext,
                int Page = 1,
                int Limit = 10) =>
                {
                    var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub);

                    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                        return Results.Unauthorized();

                    var command = new GetNotificationsRequest(userId, request.Page, request.Limit, request.IsRead);

                    var result = await sender.Send(command);

                    return Results.Ok(result);
                })
            .WithName("GetNotifications")
            .Produces<List<NotificationResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get notifications")
            .WithDescription("Get list of user's notifications");
        }
    }
}
