using System.Security.Claims;

namespace Chat.API.Features.EditMessage
{
    public class EditMessageEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/chats/{chatSessionId}/messages/{messageId}", async (
               Guid chatSessionId,
               Guid messageId,
               EditMessageRequest request,
               ISender sender,
               HttpContext httpContext) =>
            {
                // Tạo instance trực tiếp của validator
                var validator = new EditMessageRequestValidator();
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(validationResult.Errors);
                }

                var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user ID in token");

                var command = new EditMessageCommand(chatSessionId, messageId, request.MessageText, userId);
                await sender.Send(command);
                return Results.Ok();
            })
           .RequireAuthorization()
           .WithName("EditMessage");
        }
    }

    public class EditMessageRequestValidator : AbstractValidator<EditMessageRequest>
    {
        public EditMessageRequestValidator()
        {
            RuleFor(x => x.MessageText)
                .NotEmpty().WithMessage("Message text must not be empty.")
                .MaximumLength(500).WithMessage("Message text must not exceed 500 characters.");
        }
    }

    public record EditMessageRequest(string MessageText);
}