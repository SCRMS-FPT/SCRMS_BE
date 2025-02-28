using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Notification.API.Features.SendNotification
{
    public record SendNotificationRequest(
        Guid SendTo,
        string Title,
        string Content,
        string Type, 
        Boolean SendMail,
        string UserEmail
        );
    public class SendNotificationEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/notifications/send",
                async ([FromBody] SendNotificationRequest request, [FromServices] ISender sender, HttpContext httpContext) =>
                {

                    var command = new SendNotificationCommand(request.SendTo, request.Title, request.Content, request.Type, request.SendMail, request.UserEmail);

                    var result = await sender.Send(command);

                    return Results.Ok();
                })
            .WithName("SendNotification")
            .Produces<SendNotificationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Send notification")
            .WithDescription("Send notification to user");
        }
    }
}
