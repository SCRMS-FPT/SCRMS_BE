using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;

namespace Notification.API.Features.GetNotifications
{
    public class GetNotificationsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/notifications",
                async ([FromServices] ISender sender,
                HttpContext httpContext, string? Type, Boolean? IsRead,
                int Page = 1,
                int Limit = 10) =>
                {
                    var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                        ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                        return Results.Unauthorized();

                    var command = new GetNotificationsQuery(userId, Page, Limit, IsRead, Type);

                    var result = await sender.Send(command);

                    return Results.Ok(result);
                })
            .WithName("GetNotifications")
            .Produces<PaginatedResult<NotificationResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get notifications")
            .WithDescription("Get list of user's notifications");
        }
    }
}
