using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Notification.Api.Features.SendEmail;

namespace Notification.API.Features.SendEmail
{
    public record SendEmailRequest(string to, string subject, string body);
    public class SendEmailEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/sendmail",
                async ([FromBody] SendEmailRequest request, [FromServices] ISender sender, HttpContext httpContext) =>
                {
                    //var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub);

                    //if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachUserId))
                    //    return Results.Unauthorized();

                    var command = new SendEmailCommand(request.to, request.subject, request.body);

                    var result = await sender.Send(command);

                    return Results.Ok();
                })
            .WithName("SendMail")
            .Produces<SendEmailResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Send mail")
            .WithDescription("Send mail to user using GMAIL");
        }
    }
}
