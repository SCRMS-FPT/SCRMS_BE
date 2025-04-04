﻿using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Notification.API.Features.ReadNotification
{
    public class ReadNotificationsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/notifications/{notificationId:guid}/read",
                async (
                [FromServices] ISender sender,
                HttpContext httpContext,
                Guid notificationId) =>
                {
                    var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                        ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                        return Results.Unauthorized();

                    var command = new ReadNotificationCommand(notificationId, userId);

                    var result = await sender.Send(command);

                    return Results.Ok();
                })
            .WithName("ReadNotification")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Mark a notification as read")
            .WithDescription("Updates the notification status to read for the user");
        }
    }
}
