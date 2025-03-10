using System.Security.Claims;

namespace Chat.API.Features.MarkMessageAsRead
{
    public class MarkMessageAsReadEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/chats/{chatSessionId}/messages/{messageId}/read", async (
                Guid chatSessionId,
                Guid messageId,
                ISender sender,
                HttpContext httpContext) =>
            {
                var parameters = new MarkMessageAsReadParameters(chatSessionId, messageId);
                var validator = new MarkMessageAsReadParametersValidator();
                var validationResult = await validator.ValidateAsync(parameters);
                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(validationResult.Errors);
                }

                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user ID in token");

                var command = new MarkMessageAsReadCommand(chatSessionId, messageId, userId);
                await sender.Send(command);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("MarkMessageAsRead");
        }

        public record MarkMessageAsReadParameters(Guid ChatSessionId, Guid MessageId);

        public class MarkMessageAsReadParametersValidator : AbstractValidator<MarkMessageAsReadParameters>
        {
            public MarkMessageAsReadParametersValidator()
            {
                RuleFor(x => x.ChatSessionId)
                    .NotEmpty().WithMessage("ChatSessionId cannot be empty.");
                RuleFor(x => x.MessageId)
                    .NotEmpty().WithMessage("MessageId cannot be empty.");
            }
        }
    }
}