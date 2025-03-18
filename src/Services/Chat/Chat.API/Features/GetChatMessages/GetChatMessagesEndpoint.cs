namespace Chat.API.Features.GetChatMessages
{
    public class GetChatMessagesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/chats/{chatSessionId}/messages", async (
               Guid chatSessionId,
               int page,
               int limit,
               ISender sender) =>
            {
                var query = new GetChatMessagesQuery(chatSessionId, page, limit);
                var validator = new GetChatMessagesQueryValidator();
                var validationResult = await validator.ValidateAsync(query);
                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(validationResult.Errors);
                }

                var result = await sender.Send(query);
                return Results.Ok(result);
            })
           .RequireAuthorization()
           .WithName("GetChatMessages");
        }
    }

    public class GetChatMessagesQueryValidator : AbstractValidator<GetChatMessagesQuery>
    {
        public GetChatMessagesQueryValidator()
        {
            RuleFor(x => x.ChatSessionId)
                .NotEmpty().WithMessage("ChatSessionId cannot be empty.");
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0.");
            RuleFor(x => x.Limit)
                .GreaterThan(0).WithMessage("Limit must be greater than 0.");
        }
    }
}